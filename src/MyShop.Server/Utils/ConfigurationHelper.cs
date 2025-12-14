using MyShop.Server.Services.Implementations;

namespace MyShop.Server.Utils
{
    /// <summary>
    /// Utility class to generate encoded configuration values.
    /// Call GenerateEncodedConfig() to get encoded values for your appsettings.json.
    /// 
    /// CONFIGURATION GUIDE:
    /// ====================
    /// MyShop Server now uses a comprehensive configuration system.
    /// All hardcoded values have been moved to appsettings.json.
    /// 
    /// Configuration Sections:
    /// ----------------------
    /// 1. AppSettings - General application settings (BaseUrl, file uploads, pagination)
    /// 2. JwtSettings - JWT authentication (token expiry, secret key)
    /// 3. EmailSettings - Email service (Brevo API credentials)
    /// 4. BusinessSettings - Business rules (tax, shipping, commissions)
    /// 5. VerificationSettings - Email verification and OTP settings
    /// 
    /// See CONFIGURATION_GUIDE.md for detailed documentation.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Generates encoded configuration values and prints them to console.
        /// This is a utility method, not an entry point.
        /// </summary>
        public static void GenerateEncodedConfig()
        {
            Console.WriteLine("=== Configuration Encoder ===");
            Console.WriteLine();

            // Encode Email Endpoint
            var originalEndpoint = "https://api.brevo.com/v3/smtp/email";
            var encodedEndpoint = ConfigurationEncoder.Encode(originalEndpoint);
            
            Console.WriteLine("Email API Endpoint:");
            Console.WriteLine($"Original: {originalEndpoint}");
            Console.WriteLine($"Encoded:  {encodedEndpoint}");
            Console.WriteLine();

            // Encode API Key (example - replace with your actual key)
            var originalApiKey = "xkeysib-YOUR-API-KEY-HERE";
            var encodedApiKey = ConfigurationEncoder.Encode(originalApiKey);
            
            Console.WriteLine("Email API Key:");
            Console.WriteLine($"Original: {originalApiKey}");
            Console.WriteLine($"Encoded:  {encodedApiKey}");
            Console.WriteLine();

            // Test decoding
            Console.WriteLine("=== Verification ===");
            Console.WriteLine($"Decoded Endpoint: {ConfigurationEncoder.Decode(encodedEndpoint)}");
            Console.WriteLine($"Decoded API Key:  {ConfigurationEncoder.Decode(encodedApiKey)}");
            Console.WriteLine();

            Console.WriteLine("=== Copy this to your appsettings.json ===");
            Console.WriteLine("\"EmailSettings\": {");
            Console.WriteLine($"  \"ApiEndpoint\": \"{encodedEndpoint}\",");
            Console.WriteLine($"  \"ApiKey\": \"{encodedApiKey}\",");
            Console.WriteLine("  \"SenderName\": \"MyShop\",");
            Console.WriteLine("  \"SenderEmail\": \"noreply@myshop.com\",");
            Console.WriteLine("  \"TemplatesPath\": \"EmailTemplates\"");
            Console.WriteLine("}");
            Console.WriteLine();
            
            Console.WriteLine("=== Configuration Guide ===");
            Console.WriteLine("All configuration settings are now in appsettings.json:");
            Console.WriteLine();
            Console.WriteLine("1. BusinessSettings - Tax, shipping, commissions");
            Console.WriteLine("   - TaxRate: 0.1 (10%)");
            Console.WriteLine("   - ShippingFee: 30000 VND");
            Console.WriteLine("   - FreeShippingThreshold: 500000 VND");
            Console.WriteLine("   - CommissionRate: 0.05 (5%)");
            Console.WriteLine();
            Console.WriteLine("2. VerificationSettings - Email verification");
            Console.WriteLine("   - EmailTokenExpirationHours: 24");
            Console.WriteLine("   - OtpCodeLength: 6");
            Console.WriteLine("   - OtpExpirationMinutes: 10");
            Console.WriteLine();
            Console.WriteLine("3. AppSettings - General settings");
            Console.WriteLine("   - BaseUrl: http://localhost:5228");
            Console.WriteLine("   - MaxFileUploadSizeMB: 5");
            Console.WriteLine("   - DefaultPageSize: 10");
            Console.WriteLine();
            Console.WriteLine("See CONFIGURATION_GUIDE.md for complete documentation.");
        }
    }
}