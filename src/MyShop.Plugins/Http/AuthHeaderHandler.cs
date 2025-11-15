using MyShop.Core.Interfaces.Storage;

namespace MyShop.Plugins.Http;

/// <summary>
/// HTTP handler để tự động inject JWT token vào Authorization header
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ICredentialStorage _credentialStorage;

    public AuthHeaderHandler(ICredentialStorage credentialStorage)
    {
        _credentialStorage = credentialStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _credentialStorage.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
