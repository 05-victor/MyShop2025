using MyShop.Core.Common;
using MyShop.Core.Interfaces.Infrastructure;
using Windows.Security.Credentials;

namespace MyShop.Plugins.Infrastructure;

/// <summary>
/// Windows PasswordVault implementation for secure credential storage
/// Sử dụng Windows Credential Manager - bảo mật cao, phù hợp production
/// </summary>
public class WindowsCredentialStorage : ICredentialStorage
{
    private const string ResourceName = "MyShop2025JwtToken";

    public async Task<Result<Unit>> SaveToken(string token)
    {
        try
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
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to save token: {ex.Message}");
        }
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

    public async Task<Result<Unit>> RemoveToken()
    {
        try
        {
            var credential = GetCredential();
            if (credential != null)
            {
                var vault = new PasswordVault();
                vault.Remove(credential);
            }
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to remove token: {ex.Message}");
        }
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
