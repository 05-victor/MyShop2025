using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Service to check if the API server is reachable.
    /// Used on app startup to warn users if server is offline.
    /// </summary>
    public class ApiHealthCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiHealthCheckService(string baseUrl, int timeoutSeconds = 5)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
        }

        /// <summary>
        /// Checks if the API server is reachable.
        /// Returns true if server responds (any status code), false if unreachable.
        /// </summary>
        public async Task<(bool IsReachable, string Message)> CheckHealthAsync()
        {
            try
            {
                // Try to ping the base URL or /health endpoint
                var healthUrl = $"{_baseUrl}/health";
                
                System.Diagnostics.Debug.WriteLine($"[HealthCheck] Pinging: {healthUrl}");
                
                var response = await _httpClient.GetAsync(healthUrl);
                
                // Any response (even 404) means server is running
                System.Diagnostics.Debug.WriteLine($"[HealthCheck] ✓ Server responded: {response.StatusCode}");
                return (true, $"Server is online ({response.StatusCode})");
            }
            catch (TaskCanceledException)
            {
                // Timeout
                System.Diagnostics.Debug.WriteLine($"[HealthCheck] ✗ Timeout - Server not responding");
                return (false, $"Server timeout. Please check if API is running at {_baseUrl}");
            }
            catch (HttpRequestException ex)
            {
                // Connection failed
                System.Diagnostics.Debug.WriteLine($"[HealthCheck] ✗ Connection failed: {ex.Message}");
                return (false, $"Cannot connect to API server at {_baseUrl}.\nError: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Other errors
                System.Diagnostics.Debug.WriteLine($"[HealthCheck] ✗ Unexpected error: {ex.Message}");
                return (false, $"Health check failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
