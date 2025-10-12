# Role Selection Feature - Implementation Summary

## Overview

Added functionality to allow users to specify their roles during registration and retrieve available roles from the backend.

## Changes Made

### 1. Data Layer

#### New Files Created:

- **`IRoleRepository.cs`** - Interface for role repository operations

  - `GetByNameAsync(string name)` - Get role by name
  - `GetAllAsync()` - Get all available roles
  - `ExistsAsync(string name)` - Check if role exists

- **`RoleRepository.cs`** - Implementation of role repository
  - Fetches roles from database with authority relationships
  - Uses Entity Framework Core with Include statements

#### Modified Files:

- **`UserRepository.cs`**
  - Updated `GetByUsernameAsync()` to include Roles navigation property
  - Updated `GetByEmailAsync()` to include Roles navigation property
  - Updated `CreateAsync()` to load Roles after saving user

### 2. Shared DTOs Layer

#### New Files Created:

- **`RoleResponse.cs`** - Response DTO for role information
  - `Name` - Role name
  - `Description` - Role description (optional)

#### Modified Files:

- **`CreateUserRequest.cs`**
  - Added `RoleNames` property (List<string>) to allow role selection during registration
  - Property is optional - empty list means no roles assigned

### 3. Service Layer

#### Modified Files:

- **`IAuthService.cs`**

  - Added `GetRolesAsync()` method signature to retrieve all available roles

- **`AuthService.cs`**
  - Updated constructor to inject `IRoleRepository`
  - Updated `RegisterAsync()` method:
    - Validates provided role names exist in database
    - Throws exception if invalid role name provided
    - Assigns roles to user during creation
    - Logs assigned roles
  - Added `GetRolesAsync()` implementation to fetch all roles from repository

### 4. Controller Layer

#### Modified Files:

- **`AuthController.cs`**
  - Added new `GET /api/v1/auth/roles` endpoint
    - Returns list of all available roles
    - Returns standardized ApiResponse with roles
    - Includes proper error handling and logging

### 5. Dependency Injection

#### Modified Files:

- **`Program.cs`**
  - Registered `IRoleRepository` and `RoleRepository` in DI container

## API Endpoints

### 1. Register User (Modified)

```
POST /api/v1/auth/register
```

**Request Body:**

```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "securePassword123",
  "phoneNumber": "+1234567890",
  "avatar": "https://example.com/avatar.jpg",
  "activateTrial": true,
  "roleNames": ["Admin", "SalesAgent"] // NEW: Optional array of role names
}
```

**Response:**

```json
{
  "success": true,
  "message": "User registered successfully",
  "statusCode": 200,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "john_doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "createdAt": "2025-10-11T10:30:00Z",
    "avatar": "https://example.com/avatar.jpg",
    "activateTrial": true,
    "isVerified": false,
    "roleNames": ["Admin", "SalesAgent"] // Returns assigned roles
  }
}
```

### 2. Get Available Roles (New)

```
GET /api/v1/auth/roles
```

**Response:**

```json
{
  "success": true,
  "message": "Roles retrieved successfully",
  "statusCode": 200,
  "data": [
    {
      "name": "Admin",
      "description": null
    },
    {
      "name": "SalesAgent",
      "description": null
    }
  ]
}
```

## Validation & Error Handling

### Registration with Roles:

- If an invalid role name is provided, returns 400 Bad Request with error message
- Example: `"Role 'InvalidRole' does not exist"`
- If no roles provided in request, user is created without any roles
- All roles are validated before user creation

## Database Schema

The implementation uses the existing database schema:

- **Users** table - stores user information
- **Roles** table - stores available roles (seeded with "Admin" and "SalesAgent")
- **user_roles** table - many-to-many junction table linking users and roles

## Usage Examples

### Example 1: Register user with Admin role

```bash
POST /api/v1/auth/register
{
  "username": "admin_user",
  "email": "admin@example.com",
  "password": "password123",
  "phoneNumber": "+1234567890",
  "roleNames": ["Admin"]
}
```

### Example 2: Register user without roles

```bash
POST /api/v1/auth/register
{
  "username": "regular_user",
  "email": "user@example.com",
  "password": "password123",
  "phoneNumber": "+1234567890",
  "roleNames": []  // or omit this field
}
```

### Example 3: Get available roles before registration

```bash
GET /api/v1/auth/roles
```

## Testing Recommendations

1. Test registration with valid role names
2. Test registration with invalid role names (should return 400)
3. Test registration without roles (should succeed with empty role list)
4. Test GET /api/v1/auth/roles endpoint
5. Test login to verify roles are returned in response
6. Test with multiple roles assigned to single user

## Notes

- The existing seeded roles are: "Admin" and "SalesAgent"
- Role names are case-sensitive when matching
- Multiple roles can be assigned to a single user
- Login response also includes role information (existing functionality)
