using System.Text;

namespace MyShop.Server.Services.Implementations
{
    /// <summary>
    /// Service for encoding and decoding sensitive configuration values
    /// </summary>
    public static class ConfigurationEncoder
    {
        /// <summary>
        /// Encode a string value to Base64
        /// </summary>
        /// <param name="value">Original string value</param>
        /// <returns>Base64 encoded string</returns>
        public static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decode a Base64 string back to original value
        /// </summary>
        /// <param name="encodedValue">Base64 encoded string</param>
        /// <returns>Original string value</returns>
        public static string Decode(string encodedValue)
        {
            if (string.IsNullOrEmpty(encodedValue))
                return string.Empty;

            try
            {
                var bytes = Convert.FromBase64String(encodedValue);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                // If decoding fails, return the original value (backward compatibility)
                return encodedValue;
            }
        }

        /// <summary>
        /// Generate encoded values for configuration
        /// Use this method to generate encoded values during development
        /// </summary>
        public static void GenerateEncodedValues()
        {
            var originalEndpoint = "https://api.brevo.com/v3/smtp/email";
            var encodedEndpoint = Encode(originalEndpoint);
            
            Console.WriteLine($"Original: {originalEndpoint}");
            Console.WriteLine($"Encoded: {encodedEndpoint}");
            
            // You can add more values to encode here
            var originalApiKey = "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";
            var encodedApiKey = Encode(originalApiKey);
            
            Console.WriteLine($"Original API Key: {originalApiKey}");
            Console.WriteLine($"Encoded API Key: {encodedApiKey}");
        }
    }
}