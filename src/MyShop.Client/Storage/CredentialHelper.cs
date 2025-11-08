using Windows.Security.Credentials;

namespace MyShop.Client.Storage;

/// <summary>
/// Helper để lưu/đọc JWT token từ Windows PasswordVault
/// </summary>
public static class CredentialHelper
{
    private const string ResourceName = "MyShopJwtToken";

    public static void SaveToken(string token)
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

    public static string? GetToken()
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

    public static void RemoveToken()
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

    private static PasswordCredential? GetCredential()
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
