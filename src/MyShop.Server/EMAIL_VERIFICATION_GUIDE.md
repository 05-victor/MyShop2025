# Email Verification Feature Documentation

## Overview

The email verification feature allows users to verify their email addresses through a secure token-based system. When a user requests verification, they receive an email with a unique, time-limited verification link.

## Architecture

### Components

1. **IEmailVerificationService** (`Services/Interfaces/IEmailVerificationService.cs`)
   - Interface defining email verification operations
2. **EmailVerificationService** (`Services/Implementations/EmailVerificationService.cs`)
   - Implements verification logic using JWT tokens
   - Integrates with EmailNotificationService for sending emails
3. **EmailVerificationController** (`Controllers/EmailVerificationController.cs`)

   - Provides REST API endpoints for verification operations

4. **Email Template** (`EmailTemplates/email-verification.html`)
   - HTML template for the verification email

### Security Features

#### JWT Token-Based Verification

- **Token Type**: JWT (JSON Web Token)
- **Signing Algorithm**: HMACSHA256
- **Expiration**: 24 hours
- **Claims**:
  - `userId`: The user's unique identifier
  - `purpose`: Set to "email_verification" to prevent token reuse
  - `jti`: Unique token identifier

#### Token Validation

- Validates issuer and audience
- Checks token expiration
- Verifies token purpose
- Ensures token signature is valid

## API Endpoints

### 1. Send Verification Email (Authenticated User)

```http
POST /api/email-verification/send
Authorization: Bearer {jwt_token}
```

**Description**: Sends a verification email to the currently authenticated user.

**Response**:

```json
{
  "message": "Verification email sent successfully"
}
```

**Error Responses**:

- `400 Bad Request`: Email is already verified or user not found
- `401 Unauthorized`: Invalid or missing JWT token
- `500 Internal Server Error`: Server error

### 2. Send Verification Email (Admin)

```http
POST /api/email-verification/send/{userId}
Authorization: Bearer {admin_jwt_token}
```

**Description**: Allows administrators to send verification emails to specific users.

**Parameters**:

- `userId` (path): The GUID of the user to send verification email to

**Response**:

```json
{
  "message": "Verification email sent successfully"
}
```

**Error Responses**:

- `400 Bad Request`: Email is already verified or user not found
- `401 Unauthorized`: Invalid or missing JWT token
- `403 Forbidden`: User is not an admin
- `500 Internal Server Error`: Server error

### 3. Verify Email

```http
GET /api/email-verification/verify?token={verification_token}
```

**Description**: Verifies a user's email address using the token from the verification URL. This endpoint is publicly accessible.

**Parameters**:

- `token` (query): The verification token from the email

**Response**:

```json
{
  "message": "Email verified successfully",
  "success": true
}
```

**Error Responses**:

- `400 Bad Request`: Invalid or expired token, or user not found
- `500 Internal Server Error`: Server error

### 4. Check Verification Status

```http
GET /api/email-verification/status
Authorization: Bearer {jwt_token}
```

**Description**: Returns the current user's email verification status.

**Response**:

```json
{
  "userId": "guid-here",
  "message": "Verification status endpoint"
}
```

## Configuration

### appsettings.json

```json
{
  "AppSettings": {
    "BaseUrl": "https://localhost:5228"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters",
    "Issuer": "MyShop.Server",
    "Audience": "MyShop.Client"
  },
  "EmailSettings": {
    "ApiEndpoint": "base64_encoded_brevo_endpoint",
    "ApiKey": "base64_encoded_brevo_api_key",
    "SenderName": "MyShop",
    "SenderEmail": "noreply@myshop.com",
    "TemplatesPath": "EmailTemplates"
  }
}
```

## Email Template

The email template (`EmailTemplates/email-verification.html`) uses named placeholders:

- `{{USERNAME}}`: User's username
- `{{EMAIL}}`: User's email address
- `{{VERIFICATION_URL}}`: The verification link with token
- `{{EXPIRATION_HOURS}}`: Token expiration time (24 hours)

## Implementation Flow

### Sending Verification Email

```mermaid
sequenceDiagram
    User->>+API: POST /api/email-verification/send
    API->>+EmailVerificationService: SendVerificationEmailAsync(userId)
    EmailVerificationService->>+UserRepository: GetByIdAsync(userId)
    UserRepository-->>-EmailVerificationService: User entity
    EmailVerificationService->>EmailVerificationService: GenerateVerificationToken(userId)
    EmailVerificationService->>+EmailNotificationService: SendEmailAsync(...)
    EmailNotificationService->>EmailNotificationService: Process template
    EmailNotificationService->>+Brevo API: Send email
    Brevo API-->>-EmailNotificationService: Success
    EmailNotificationService-->>-EmailVerificationService: true
    EmailVerificationService-->>-API: ServiceResult.Success
    API-->>-User: 200 OK
```

### Verifying Email

```mermaid
sequenceDiagram
    User->>+Browser: Click verification link
    Browser->>+API: GET /api/email-verification/verify?token=...
    API->>+EmailVerificationService: VerifyEmailAsync(token)
    EmailVerificationService->>EmailVerificationService: ValidateVerificationToken(token)
    EmailVerificationService->>+UserRepository: GetByIdAsync(userId)
    UserRepository-->>-EmailVerificationService: User entity
    EmailVerificationService->>EmailVerificationService: Set IsEmailVerified = true
    EmailVerificationService->>+UserRepository: UpdateAsync(user)
    UserRepository-->>-EmailVerificationService: Updated user
    EmailVerificationService-->>-API: ServiceResult.Success
    API-->>-Browser: 200 OK with success message
```

## Database Schema

The `User` entity includes:

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? UpdatedAt { get; set; }
    // ... other properties
}
```

## Usage Examples

### Example 1: User Requests Verification Email

```bash
curl -X POST https://localhost:5228/api/email-verification/send \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

### Example 2: User Clicks Verification Link

The user receives an email with a link like:

```
https://localhost:5228/api/email-verification/verify?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

When clicked, this GET request verifies the email.

### Example 3: Admin Sends Verification to User

```bash
curl -X POST https://localhost:5228/api/email-verification/send/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer admin_token_here"
```

## Error Handling

### Common Scenarios

1. **Token Expired**

   - Message: "Invalid or expired verification token"
   - Action: User should request a new verification email

2. **Already Verified**

   - Message: "Email is already verified"
   - Action: User can proceed without further action

3. **User Not Found**

   - Message: "User not found"
   - Action: Check if user account still exists

4. **Email Send Failure**
   - Message: "Failed to send verification email"
   - Action: Check email service configuration and API credentials

## Testing

### Manual Testing Steps

1. **Register a new user** (if not using existing user)
2. **Authenticate** to get JWT token
3. **Send verification email**:
   ```bash
   curl -X POST https://localhost:5228/api/email-verification/send \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```
4. **Check email** for verification link
5. **Click the verification link** or make GET request to verify endpoint
6. **Verify in database** that `IsEmailVerified` is now `true`

### Unit Testing Considerations

Test cases should cover:

- ✅ Token generation and validation
- ✅ Expired token handling
- ✅ Invalid token handling
- ✅ Already verified user handling
- ✅ Non-existent user handling
- ✅ Email sending failure handling

## Security Best Practices

1. **Token Expiration**: Tokens expire after 24 hours
2. **Single-Purpose Tokens**: Tokens include a `purpose` claim to prevent misuse
3. **HTTPS Only**: Always use HTTPS in production for verification URLs
4. **Rate Limiting**: Consider implementing rate limiting on send endpoint
5. **Unique Token IDs**: Each token has a unique `jti` claim

## Deployment Checklist

- [ ] Update `AppSettings.BaseUrl` to production URL
- [ ] Configure proper SMTP/Email service credentials
- [ ] Ensure JWT secret key is strong and unique
- [ ] Set up HTTPS certificates
- [ ] Configure email template branding
- [ ] Test email delivery in production environment
- [ ] Set up email service monitoring
- [ ] Configure logging for verification events

## Troubleshooting

### Email Not Received

1. Check email service logs
2. Verify email API credentials
3. Check spam/junk folder
4. Verify email template path configuration

### Token Validation Fails

1. Check JWT configuration matches between token generation and validation
2. Verify token hasn't expired
3. Check system clock synchronization
4. Review JWT secret key configuration

### Database Update Fails

1. Check database connection
2. Verify user exists
3. Review database permissions
4. Check for concurrent update conflicts

## Related Documentation

- [EmailNotificationService Documentation](./EMAIL_NOTIFICATION_SERVICE.md)
- [JWT Configuration Guide](./DEBUG_JWT_GUIDE.md)
- [User Authentication Guide](./AUTHORIZE_ATTRIBUTE_GUIDE.md)
