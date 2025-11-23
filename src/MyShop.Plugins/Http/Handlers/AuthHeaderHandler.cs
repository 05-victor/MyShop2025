using MyShop.Core.Interfaces.Infrastructure;
using System.Net;
using System.Net.Http.Headers;

namespace MyShop.Plugins.Http.Handlers;

/// <summary>
/// HTTP handler để tự động inject JWT token vào Authorization header
/// Xử lý token refresh khi gặp 401 Unauthorized
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ICredentialStorage _credentialStorage;
    private bool _isRefreshing = false;

    public AuthHeaderHandler(ICredentialStorage credentialStorage)
    {
        _credentialStorage = credentialStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Step 1: Add current token to request
        var token = _credentialStorage.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Step 2: Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // Step 3: Handle 401 Unauthorized - Token Refresh
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] 401 Unauthorized - Attempting token refresh...");

            // Prevent multiple refresh attempts
            _isRefreshing = true;

            try
            {
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
                    // Clear invalid token
                    _credentialStorage.RemoveToken();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthHeaderHandler] Token refresh error: {ex.Message}");
                _credentialStorage.RemoveToken();
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        return response;
    }

    /// <summary>
    /// Refresh the access token using refresh token
    /// TODO: Replace with actual refresh token API call when backend implements it
    /// </summary>
    private async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Mock implementation - In production, this would call:
            // POST /api/auth/refresh-token
            // Body: { refreshToken: "stored_refresh_token" }
            // Response: { accessToken: "new_access_token", refreshToken: "new_refresh_token" }

            System.Diagnostics.Debug.WriteLine("[AuthHeaderHandler] Mock refresh - In production, would call refresh token endpoint");

            // Simulate network delay
            await Task.Delay(500, cancellationToken);

            // For now, return false to force re-login
            // When backend implements refresh token endpoint, implement actual logic
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
}
