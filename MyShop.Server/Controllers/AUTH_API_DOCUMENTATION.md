# Authentication REST API Documentation

## Overview
This REST API provides authentication endpoints for user registration, login, and profile retrieval. The API follows a clean architecture with proper separation of concerns:

- **DTOs** (Data Transfer Objects): Request/Response models
- **Repositories**: Data access layer
- **Services**: Business logic layer
- **Controllers**: API endpoints

## Architecture

```
???????????????
? Controller  ?  ? HTTP Requests/Responses
???????????????
       ?
???????????????
?  Service    ?  ? Business Logic
???????????????
       ?
???????????????
? Repository  ?  ? Data Access
???????????????
       ?
???????????????
?  Database   ?
???????????????
```

## Base URL
```
https://localhost:{port}/api/auth
```

## Endpoints

### 1. Register New User

**Endpoint:** `POST /api/auth/register`

**Description:** Creates a new user account.

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePass123",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "photo": "https://example.com/photo.jpg"
}
```

**Validation Rules:**
- `username`: Required, 3-50 characters
- `password`: Required, minimum 6 characters
- `fullName`: Required, maximum 100 characters
- `email`: Required, valid email format
- `photo`: Optional

**Success Response (200 OK):**
```json
{
  "id": 1,
  "username": "john_doe",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "photo": "https://example.com/photo.jpg",
  "role": "user",
  "token": "",
  "message": "User registered successfully"
}
```

**Error Response (400 Bad Request):**
```json
{
  "id": 0,
  "username": "",
  "fullName": "",
  "email": "",
  "photo": null,
  "role": "",
  "token": "",
  "message": "Username or email already exists"
}
```

**cURL Example:**
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "password": "SecurePass123",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "photo": ""
  }'
```

---

### 2. Login

**Endpoint:** `POST /api/auth/login`

**Description:** Authenticates a user and returns user details.

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePass123"
}
```

**Validation Rules:**
- `username`: Required
- `password`: Required

**Success Response (200 OK):**
```json
{
  "id": 1,
  "username": "john_doe",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "photo": "https://example.com/photo.jpg",
  "role": "user",
  "token": "",
  "message": "Login successful"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "id": 0,
  "username": "",
  "fullName": "",
  "email": "",
  "photo": null,
  "role": "",
  "token": "",
  "message": "Invalid username or password"
}
```

**cURL Example:**
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "password": "SecurePass123"
  }'
```

---

### 3. Get Current User (GetMe)

**Endpoint:** `GET /api/auth/me`

**Description:** Returns the currently authenticated user's profile. This endpoint is a placeholder and will be fully functional once JWT authentication is implemented.

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "username": "john_doe",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "photo": "https://example.com/photo.jpg",
  "role": "user",
  "createdAt": "2025-01-06T10:30:00Z",
  "updatedAt": "2025-01-06T10:30:00Z"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "message": "Authentication required. This endpoint will be functional once authentication is implemented."
}
```

**cURL Example:**
```bash
curl -X GET https://localhost:5001/api/auth/me \
  -H "Authorization: Bearer {your_jwt_token}"
```

---

## Testing with Different Tools

### Postman

1. **Register:**
   - Method: POST
   - URL: `https://localhost:{port}/api/auth/register`
   - Headers: `Content-Type: application/json`
   - Body (raw JSON):
   ```json
   {
     "username": "testuser",
     "password": "Test123456",
     "fullName": "Test User",
     "email": "test@example.com",
     "photo": ""
   }
   ```

2. **Login:**
   - Method: POST
   - URL: `https://localhost:{port}/api/auth/login`
   - Headers: `Content-Type: application/json`
   - Body (raw JSON):
   ```json
   {
     "username": "testuser",
     "password": "Test123456"
   }
   ```

3. **Get Me:**
   - Method: GET
   - URL: `https://localhost:{port}/api/auth/me`
   - Headers: `Authorization: Bearer {token}` (when implemented)

### HTTP Client (C#)

```csharp
using System.Net.Http.Json;
using MyShop.Server.DTOs.Auth;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Register
var registerRequest = new RegisterRequestDto
{
    Username = "john_doe",
    Password = "SecurePass123",
    FullName = "John Doe",
    Email = "john.doe@example.com",
    Photo = ""
};

var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

Console.WriteLine($"Registration: {registerResult?.Message}");

// Login
var loginRequest = new LoginRequestDto
{
    Username = "john_doe",
    Password = "SecurePass123"
};

var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

Console.WriteLine($"Login: {loginResult?.Message}");
Console.WriteLine($"User ID: {loginResult?.Id}");
```

---

## Project Structure

```
MyShop.Server/
??? Controllers/
?   ??? AuthController.cs          # REST API endpoints
??? DTOs/
?   ??? Auth/
?       ??? RegisterRequestDto.cs  # Registration input model
?       ??? LoginRequestDto.cs     # Login input model
?       ??? AuthResponseDto.cs     # Authentication response model
?       ??? UserDto.cs             # User profile model
??? Services/
?   ??? Interfaces/
?   ?   ??? IAuthService.cs        # Service interface
?   ??? AuthService.cs             # Business logic implementation
??? Repositories/
    ??? Interfaces/
    ?   ??? IUserRepository.cs     # Repository interface
    ??? UserRepository.cs          # Data access implementation
```

---

## Error Handling

All endpoints return appropriate HTTP status codes:

- **200 OK**: Request successful
- **400 Bad Request**: Validation error or business rule violation
- **401 Unauthorized**: Invalid credentials or missing authentication
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

**Error Response Format:**
```json
{
  "message": "Error description"
}
```

---

## Security Notes

?? **Current Implementation (Development Only):**
- Passwords are stored in plain text
- No JWT token generation
- No authentication middleware
- No authorization checks

? **TODO for Production:**
1. **Password Hashing**: Implement BCrypt.Net or Argon2 for password hashing
2. **JWT Authentication**: 
   - Generate JWT tokens on login
   - Validate tokens on protected endpoints
   - Add JWT middleware to Program.cs
3. **Authorization**: Add role-based access control
4. **HTTPS**: Enforce HTTPS in production
5. **Rate Limiting**: Implement rate limiting for login attempts
6. **Input Sanitization**: Add additional input validation
7. **CORS**: Configure CORS policy appropriately

---

## Next Steps

### Implementing JWT Authentication:

1. **Install Package:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

2. **Update Program.cs:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
```

3. **Add [Authorize] attribute to GetMe endpoint**

4. **Generate JWT tokens in AuthService**

---

## API Testing Checklist

- [ ] Register new user with valid data
- [ ] Register user with duplicate username
- [ ] Register user with duplicate email
- [ ] Register user with invalid email format
- [ ] Register user with short password
- [ ] Login with valid credentials
- [ ] Login with invalid username
- [ ] Login with invalid password
- [ ] Access GetMe endpoint (expect 401 for now)

---

## License
This API is part of the MyShop project.
