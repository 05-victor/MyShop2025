# Email Configuration Security - Encoded Endpoints

## ?? Overview

?? b?o m?t endpoint và API key, chúng ta ?ã implement Base64 encoding cho các giá tr? nh?y c?m trong configuration.

## ?? Current Encoded Values

### appsettings.json
```json
{
  "EmailSettings": {
    "ApiEndpoint": "aHR0cHM6Ly9hcGkuYnJldm8uY29tL3YzL3NtdHAvZW1haWw=",
    "ApiKey": "eGtleXNpYi04OGJkZDY4OWI1NGMwZmFkZjg2ZGViNWU5NmQ2YmEwNDI0YzFmNmM5NWQ4OTk5NTNjMzQ0YWMwNzVmZmUzYjM4LVdoN3FmSHRvRWxRVDJpVXY=",
    "SenderName": "MyShop",
    "SenderEmail": "mailtacvu05@gmail.com",
    "TemplatesPath": "EmailTemplates"
  }
}
```

### Decoded Values:
- **ApiEndpoint**: ``
- **ApiKey**: `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`

## ??? How It Works

### 1. Configuration Reading
```csharp
// EmailSettings automatically decodes values when accessed
public string GetDecodedApiEndpoint()
{
    return ConfigurationEncoder.Decode(ApiEndpoint);
}

public string GetDecodedApiKey()
{
    return ConfigurationEncoder.Decode(ApiKey);
}
```

### 2. Service Usage
```csharp
// EmailNotificationService uses decoded values
public EmailNotificationService(IOptions<EmailSettings> emailSettings, ...)
{
    _emailSettings = emailSettings.Value;
    
    // Uses decoded API key
    _httpClient.DefaultRequestHeaders.Add("api-key", _emailSettings.GetDecodedApiKey());
}

// Uses decoded endpoint
var response = await _httpClient.PostAsync(_emailSettings.GetDecodedApiEndpoint(), content);
```

## ?? Adding New Encoded Values

### Method 1: Using ConfigurationEncoder
```csharp
using MyShop.Server.Services.Implementations;

// Encode a new value
var originalValue = "your-secret-value";
var encodedValue = ConfigurationEncoder.Encode(originalValue);

// Decode it back
var decodedValue = ConfigurationEncoder.Decode(encodedValue);
```

### Method 2: Manual Base64 Encoding
```csharp
// Encode
var bytes = Encoding.UTF8.GetBytes("your-secret-value");
var encoded = Convert.ToBase64String(bytes);

// Decode
var decodedBytes = Convert.FromBase64String(encoded);
var decoded = Encoding.UTF8.GetString(decodedBytes);
```

## ?? Benefits

? **Obfuscated Configuration** - API endpoints and keys are not readable in plain text  
? **Version Control Safe** - Sensitive data is encoded in repository  
? **Backward Compatible** - Falls back to original value if decoding fails  
? **Simple Implementation** - Uses standard Base64 encoding  
? **No External Dependencies** - Uses built-in .NET libraries  

## ?? Security Notes

?? **Important:** Base64 is **encoding**, not encryption. It provides obfuscation, not security.

For production:
- Consider using proper encryption (AES, etc.)
- Use Azure Key Vault or similar secret management
- Implement proper access controls
- Base64 encoding just makes values less obvious to casual observers

## ?? Testing Encoded Values

### Verify Decoding Works
```csharp
var encoder = new ConfigurationEncoder();

// Test endpoint
var originalEndpoint = "";
var encoded = ConfigurationEncoder.Encode(originalEndpoint);
var decoded = ConfigurationEncoder.Decode(encoded);

Console.WriteLine($"Original: {originalEndpoint}");
Console.WriteLine($"Encoded:  {encoded}");
Console.WriteLine($"Decoded:  {decoded}");
Console.WriteLine($"Match: {originalEndpoint == decoded}");
```

## ?? Email API Still Works the Same

The Email APIs continue to work exactly as before:

```bash
POST /api/v1/email/send
{
  "recipientEmail": "test@example.com",
  "recipientName": "Test User",
  "subject": "Test Email",
  "templatePath": "welcome.html",
  "values": ["John", "johndoe123"]
}
```

The encoding/decoding happens transparently in the background.

## ?? Future Enhancements

Consider implementing:
- **AES Encryption** instead of Base64 encoding
- **Environment-specific keys** for different environments
- **Key rotation** mechanism
- **Azure Key Vault** integration
- **HashiCorp Vault** integration

## ?? Files Modified

- `EmailSettings.cs` - Added decoding methods
- `EmailNotificationService.cs` - Uses decoded values
- `ConfigurationEncoder.cs` - Encoding/decoding utility
- `ConfigurationHelper.cs` - Helper to generate encoded values
- `appsettings.json` - Contains encoded values