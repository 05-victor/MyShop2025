using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of ISystemActivationRepository
/// Delegates to MockSystemActivationData for data operations
/// </summary>
public class MockSystemActivationRepository : ISystemActivationRepository
{
    public async Task<Result<bool>> HasAnyAdminAsync()
    {
        try
        {
            var hasAdmin = await MockSystemActivationData.HasAnyAdminAsync();
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] HasAnyAdminAsync: {hasAdmin}");
            return Result<bool>.Success(hasAdmin);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] HasAnyAdminAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to check admin status: {ex.Message}");
        }
    }

    public async Task<Result<AdminCodeInfo>> ValidateCodeAsync(string code)
    {
        try
        {
            var adminCode = await MockSystemActivationData.ValidateCodeAsync(code);

            if (adminCode == null)
            {
                return Result<AdminCodeInfo>.Failure("Invalid or already used activation code");
            }

            var codeInfo = new AdminCodeInfo
            {
                Code = adminCode.Code,
                Type = adminCode.Type,
                DurationDays = adminCode.DurationDays,
                Description = adminCode.Description,
                IsValid = true
            };

            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] ValidateCodeAsync: {code} -> Valid");
            return Result<AdminCodeInfo>.Success(codeInfo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] ValidateCodeAsync error: {ex.Message}");
            return Result<AdminCodeInfo>.Failure($"Failed to validate code: {ex.Message}");
        }
    }

    public async Task<Result<LicenseInfo>> ActivateCodeAsync(string code, Guid userId)
    {
        try
        {
            var (success, errorMessage, license) = await MockSystemActivationData.ActivateCodeAsync(code, userId.ToString());

            if (!success || license == null)
            {
                return Result<LicenseInfo>.Failure(errorMessage ?? "Activation failed");
            }

            var remainingDays = await MockSystemActivationData.GetRemainingTrialDaysAsync();
            var isExpiring = await MockSystemActivationData.IsTrialExpiringAsync();

            var licenseInfo = new LicenseInfo
            {
                UserId = Guid.Parse(license.UserId),
                Type = license.Type,
                ActivatedAt = license.ActivatedAt,
                ExpiresAt = license.ExpiresAt,
                DurationDays = license.DurationDays,
                RemainingDays = remainingDays,
                IsExpired = false,
                IsExpiring = isExpiring
            };

            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] ActivateCodeAsync: {code} for {userId} -> Success");
            return Result<LicenseInfo>.Success(licenseInfo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] ActivateCodeAsync error: {ex.Message}");
            return Result<LicenseInfo>.Failure($"Failed to activate code: {ex.Message}");
        }
    }

    public async Task<Result<LicenseInfo?>> GetCurrentLicenseAsync()
    {
        try
        {
            var license = await MockSystemActivationData.GetCurrentLicenseAsync();

            if (license == null)
            {
                return Result<LicenseInfo?>.Success(null);
            }

            var remainingDays = await MockSystemActivationData.GetRemainingTrialDaysAsync();
            var isExpired = await MockSystemActivationData.IsTrialExpiredAsync();
            var isExpiring = await MockSystemActivationData.IsTrialExpiringAsync();

            var licenseInfo = new LicenseInfo
            {
                UserId = Guid.Parse(license.UserId),
                Type = license.Type,
                ActivatedAt = license.ActivatedAt,
                ExpiresAt = license.ExpiresAt,
                DurationDays = license.DurationDays,
                RemainingDays = remainingDays,
                IsExpired = isExpired,
                IsExpiring = isExpiring
            };

            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] GetCurrentLicenseAsync: Type={license.Type}, Remaining={remainingDays}");
            return Result<LicenseInfo?>.Success(licenseInfo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] GetCurrentLicenseAsync error: {ex.Message}");
            return Result<LicenseInfo?>.Failure($"Failed to get license info: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetRemainingTrialDaysAsync()
    {
        try
        {
            var remainingDays = await MockSystemActivationData.GetRemainingTrialDaysAsync();
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] GetRemainingTrialDaysAsync: {remainingDays}");
            return Result<int>.Success(remainingDays);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] GetRemainingTrialDaysAsync error: {ex.Message}");
            return Result<int>.Failure($"Failed to get remaining days: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsTrialExpiringAsync()
    {
        try
        {
            var isExpiring = await MockSystemActivationData.IsTrialExpiringAsync();
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] IsTrialExpiringAsync: {isExpiring}");
            return Result<bool>.Success(isExpiring);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] IsTrialExpiringAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to check trial expiring: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsTrialExpiredAsync()
    {
        try
        {
            var isExpired = await MockSystemActivationData.IsTrialExpiredAsync();
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] IsTrialExpiredAsync: {isExpired}");
            return Result<bool>.Success(isExpired);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] IsTrialExpiredAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to check trial expired: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DemoteExpiredAdminAsync()
    {
        try
        {
            var success = await MockSystemActivationData.DemoteAdminToCustomerAsync();

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("[MockSystemActivationRepository] DemoteExpiredAdminAsync: Success");
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Failed to demote admin");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationRepository] DemoteExpiredAdminAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to demote admin: {ex.Message}");
        }
    }
}
