using MyShop.Server.Services.Implementations;

namespace MyShop.Server.Utils
{
    /// <summary>
    /// Utility class to generate encoded configuration values.
    /// Call GenerateEncodedConfig() to get encoded values for your appsettings.json.
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
            var originalEndpoint = "";
            var encodedEndpoint = ConfigurationEncoder.Encode(originalEndpoint);
            
            Console.WriteLine("Email API Endpoint:");
            Console.WriteLine($"Original: {originalEndpoint}");
            Console.WriteLine($"Encoded:  {encodedEndpoint}");
            Console.WriteLine();

            // Encode API Key
            var originalApiKey = "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";
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
            Console.WriteLine($"\"ApiEndpoint\": \"{encodedEndpoint}\",");
            Console.WriteLine($"\"ApiKey\": \"{encodedApiKey}\",");
        }
    }
}