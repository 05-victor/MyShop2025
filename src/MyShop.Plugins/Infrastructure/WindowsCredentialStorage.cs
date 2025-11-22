using MyShop.Core.Interfaces.Infrastructure;
using Windows.Security.Credentials;

namespace MyShop.Plugins.Storage;

/// <summary>
/// Windows PasswordVault implementation for secure credential storage
/// Sử dụng Windows Credential Manager - bảo mật cao, phù hợp production
/// </summary>
public class WindowsCredentialStorage : ICredentialStorage
{
    private const string ResourceName = "MyShop2025JwtToken";

    public void SaveToken(string token)
    {
        var vault = new PasswordVault();
        try
        {
            var existingCredential = GetCredential();
            if (existingCredential != null)
            {
                vault.Remove(existingCredential);
            }
        }
        catch { }

        vault.Add(new PasswordCredential(ResourceName, "user", token));
    }

    public string? GetToken()
    {
        try
        {
            var credential = GetCredential();
            if (credential != null)
            {
                credential.RetrievePassword();
                return credential.Password;
            }
        }
        catch { }

        return null;
    }

    public void RemoveToken()
    {
        try
        {
            var credential = GetCredential();
            if (credential != null)
            {
                var vault = new PasswordVault();
                vault.Remove(credential);
            }
        }
        catch { }
    }

    private PasswordCredential? GetCredential()
    {
        var vault = new PasswordVault();
        try
        {
            return vault.FindAllByResource(ResourceName)[0];
        }
        catch
        {
            return null;
        }
    }
}
