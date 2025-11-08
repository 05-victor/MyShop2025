using MyShop.Plugins.Storage;

namespace MyShop.Plugins.Http;

/// <summary>
/// HTTP handler để tự động inject JWT token vào Authorization header
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = CredentialHelper.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
