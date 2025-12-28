using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Plugins.API.Auth;
using MyShop.Shared.DTOs.Requests;
using System.Net;
using System.Net.Http.Headers;

namespace MyShop.Plugins.Http.Handlers;

/// <summary>
/// HTTP delegating handler for automatic JWT token injection.
/// Adds Authorization header with Bearer token to all outgoing requests.
/// Handles 401 Unauthorized responses with automatic token refresh logic.
/// 
/// NOTE: Uses IServiceProvider instead of direct IAuthApi dependency
/// to avoid circular dependency during DI container initialization.
/// IAuthApi is resolved lazily only when token refresh is needed.
///
/// CRITICAL: This handler MUST be in the pipeline for ALL API clients including IAuthApi.
/// Previously IAuthApi was excluded to "avoid circular dependency" but that caused:
/// - /users/me sent without Authorization header after login
/// - 401 response caused AuthRepository to clear session tokens
/// - All subsequent API calls failed
/// The circular dependency is solved by lazy IServiceProvider resolution (see RefreshTokenAsync).
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ICredentialStorage _credentialStorage;
    private readonly IServiceProvider _serviceProvider;
    private bool _isRefreshing = false;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    public AuthHeaderHandler(ICredentialStorage credentialStorage, IServiceProvider serviceProvider)
    {
        _credentialStorage = credentialStorage;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Step 1: Add current access token to request
        var token = _credentialStorage.GetToken();
        var hasToken = !string.IsNullOrEmpty(token);
        System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler.SendAsync] {request.Method} {request.RequestUri?.PathAndQuery} - Token: {(hasToken ? "exists" : "NULL")}");

        if (hasToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler.SendAsync] Authorization header attached");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler.SendAsync] WARNING: No token available, request will be sent without Authorization header");
        }

        // Step 2: Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // Step 3: Handle 401 Unauthorized - Token Refresh
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] 401 Unauthorized - Attempting token refresh...");

            try
            {
                // Use semaphore to prevent multiple simultaneous refresh attempts
                await _refreshSemaphore.WaitAsync(cancellationToken);

                // Prevent multiple refresh attempts
                _isRefreshing = true;

                // Attempt to refresh the token
                var refreshResult = await RefreshTokenAsync(cancellationToken);

                if (refreshResult)
                {
                    System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] Token refresh successful - Retrying original request");

                    // Clone the original request (important!)
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);

                    // Add the new token
                    var newToken = _credentialStorage.GetToken();
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    }

                    // Retry the original request with new token
                    response.Dispose(); // Clean up old response
                    response = await base.SendAsync(clonedRequest, cancellationToken);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] Token refresh failed - Logout required");
                    // Token refresh failed - user needs to login again
                    // Clear invalid tokens
                    await _credentialStorage.RemoveToken();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler] Token refresh error: {ex.Message}");
                await _credentialStorage.RemoveToken();
            }
            finally
            {
                _isRefreshing = false;
                _refreshSemaphore.Release();
            }
        }

        return response;
    }

    /// <summary>
    /// Refresh the access token using refresh token
    /// </summary>
    private async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get refresh token from storage
            var refreshToken = _credentialStorage.GetRefreshToken();

            if (string.IsNullOrEmpty(refreshToken))
            {
                System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] No refresh token available");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] Calling refresh token endpoint...");

            // Lazily resolve IAuthApi only when needed (breaks circular dependency)
            var authApi = _serviceProvider.GetService(typeof(IAuthApi)) as IAuthApi;
            if (authApi == null)
            {
                System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] Failed to resolve IAuthApi from service provider");
                return false;
            }

            // Call refresh token API
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await authApi.RefreshTokenAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;

                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    var result = apiResponse.Result;

                    // Save new tokens
                    var newRefreshToken = result.RefreshToken ?? refreshToken; // Use old token if not rotated
                    await _credentialStorage.SaveToken(result.AccessToken, newRefreshToken);

                    System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] ? Token refreshed successfully");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler] ? API returned error: {apiResponse.Message}");
                    return false;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler] ? HTTP error: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler] RefreshTokenAsync exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clone HTTP request for retry (HttpRequestMessage can only be sent once)
    /// </summary>
    private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Copy content if present
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            // Copy content headers
            if (request.Content.Headers != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Copy request headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy properties
        foreach (var property in request.Options)
        {
            clone.Options.TryAdd(property.Key, property.Value);
        }

        return clone;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshSemaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}
