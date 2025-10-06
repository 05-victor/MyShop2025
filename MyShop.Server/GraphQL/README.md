# User CRUD Operations with GraphQL

## Overview
This implementation provides complete CRUD operations for the User entity using GraphQL with HotChocolate.

## GraphQL Endpoint
- **URL**: `https://localhost:{port}/graphql`
- **GraphQL Playground**: Available in development mode at the same URL

## Available Operations

### Queries

#### 1. Get All Users
```graphql
query {
  users {
    id
    username
    fullName
    email
    photo
    role
    createdAt
    updatedAt
    deletedAt
  }
}
```

#### 2. Get User by ID
```graphql
query {
  userById(id: 1) {
    id
    username
    fullName
    email
    photo
    role
    createdAt
    updatedAt
  }
}
```

#### 3. Get User by Username
```graphql
query {
  userByUsername(username: "john_doe") {
    id
    username
    fullName
    email
    role
  }
}
```

#### 4. Get User by Email
```graphql
query {
  userByEmail(email: "john@example.com") {
    id
    username
    fullName
    email
    role
  }
}
```

### Mutations

#### 1. Create User
```graphql
mutation {
  createUser(input: {
    username: "john_doe"
    password: "secure_password"
    fullName: "John Doe"
    email: "john@example.com"
    photo: "https://example.com/photo.jpg"
    role: "user"
  }) {
    id
    username
    fullName
    email
    role
    createdAt
  }
}
```

#### 2. Update User
```graphql
mutation {
  updateUser(input: {
    id: 1
    fullName: "John Updated Doe"
    email: "john.updated@example.com"
    role: "admin"
  }) {
    id
    username
    fullName
    email
    role
    updatedAt
  }
}
```

#### 3. Hard Delete User
```graphql
mutation {
  deleteUser(id: 1)
}
```

#### 4. Soft Delete User
```graphql
mutation {
  softDeleteUser(id: 1) {
    id
    username
    deletedAt
  }
}
```

## Testing with HTTP Clients

### Using cURL

**Create User:**
```bash
curl -X POST https://localhost:{port}/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "mutation { createUser(input: { username: \"john_doe\", password: \"pass123\", fullName: \"John Doe\", email: \"john@example.com\", photo: \"\", role: \"user\" }) { id username fullName email } }"
  }'
```

**Get All Users:**
```bash
curl -X POST https://localhost:{port}/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "{ users { id username fullName email role } }"
  }'
```

### Using Postman or Insomnia

1. Create a POST request to `https://localhost:{port}/graphql`
2. Set Content-Type header to `application/json`
3. In the body, use the JSON format:
```json
{
  "query": "your GraphQL query here",
  "variables": {}
}
```

## Features

? **Read Operations**:
- Get all users
- Get user by ID
- Get user by username
- Get user by email

? **Create Operation**:
- Create new user with all required fields
- Automatic timestamp management (CreatedAt, UpdatedAt)

? **Update Operation**:
- Partial updates supported (only update provided fields)
- Automatic UpdatedAt timestamp

? **Delete Operations**:
- Hard delete (permanently remove from database)
- Soft delete (set DeletedAt timestamp)

## Important Notes

?? **Security Considerations**:
- Currently, passwords are stored in plain text. **In production, always hash passwords** using a secure hashing algorithm like bcrypt or Argon2.
- Add authentication and authorization middleware before deploying to production.
- Implement input validation and sanitization.

## Next Steps

1. **Add Password Hashing**: Implement password hashing using BCrypt.Net or similar
2. **Add Authentication**: Implement JWT or cookie-based authentication
3. **Add Authorization**: Add role-based access control using HotChocolate's `[Authorize]` attribute
4. **Add Validation**: Implement input validation using FluentValidation
5. **Add Filtering & Pagination**: Use HotChocolate's filtering and pagination features
6. **Add Error Handling**: Implement custom error handling and error types

## Example Client Code (C#)

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

var query = new
{
    query = @"
        query {
            users {
                id
                username
                fullName
                email
            }
        }"
};

var response = await client.PostAsJsonAsync("/graphql", query);
var result = await response.Content.ReadFromJsonAsync<GraphQLResponse>();
```
