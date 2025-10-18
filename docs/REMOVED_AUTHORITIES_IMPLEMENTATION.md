# RemovedAuthorities Feature - Complete Implementation Guide

## 🎯 Overview

The `RemovedAuthorities` feature provides a flexible **blacklist-based authority restriction system** that allows you to remove specific authorities from individual users without modifying their roles.

### Key Concept

```
User's Effective Authorities = (Authorities from Roles) - (Removed Authorities)
```

---

## 📋 What Was Implemented

### 1. **Database Layer**

#### New Entity: `RemovedAuthorities`

- **File**: `src/MyShop.Data/Entities/RemovedAuthorities.cs`
- **Properties**:
  - `UserId` (Guid) - Foreign key to Users
  - `AuthorityName` (string) - Foreign key to Authorities
  - `Reason` (string?, optional) - Why the authority was removed
  - `RemovedAt` (DateTime) - When it was removed
  - `RemovedBy` (string?, optional) - Who removed it (admin username)
- **Primary Key**: Composite key on (UserId, AuthorityName)
- **Relationships**:
  - User → Many RemovedAuthorities (Cascade delete)
  - Authority → Many RemovedAuthorities (Restrict delete)

#### Updated Entities

- **User.cs**: Added `RemovedAuthorities` navigation property
- **ShopContext.cs**:
  - Added `DbSet<RemovedAuthorities>`
  - Configured relationships and table name (`removed_authorities`)

---

### 2. **DTOs Layer**

#### Request DTOs

- **`AddRemovedAuthorityRequest.cs`**
  ```csharp
  {
    "authorityName": "DELETE",
    "reason": "Optional reason",
    "removedBy": "admin_user"
  }
  ```

#### Response DTOs

- **`RemovedAuthorityResponse`** - Info about a removed authority
- **`EffectiveAuthoritiesResponse`** - Complete breakdown of user's authorities
- **`CheckAuthorityResponse`** - Result of authority check

---

### 3. **Service Layer**

#### Interface: `IUserAuthorityService`

- **File**: `src/MyShop.Server/Services/Interfaces/IUserAuthorityService.cs`

#### Implementation: `UserAuthorityService`

- **File**: `src/MyShop.Server/Services/Implementations/UserAuthorityService.cs`
- **Methods**:
  1. `GetEffectiveAuthoritiesAsync()` - Get list of effective authorities
  2. `GetEffectiveAuthoritiesDetailAsync()` - Get detailed breakdown
  3. `HasAuthorityAsync()` - Check if user has specific authority
  4. `AddRemovedAuthorityAsync()` - Restrict an authority
  5. `RemoveRemovedAuthorityAsync()` - Restore an authority
  6. `GetRemovedAuthoritiesAsync()` - Get list of removed authorities

---

### 4. **API Endpoints**

#### Controller: `UserAuthorityController`

- **Base Route**: `/api/v1/users/{userId}/authorities`

| Method | Endpoint                   | Description                                        |
| ------ | -------------------------- | -------------------------------------------------- |
| GET    | `/effective`               | Get user's effective authorities with full details |
| GET    | `/check/{authorityName}`   | Check if user has specific authority               |
| GET    | `/removed`                 | Get list of removed authorities for user           |
| POST   | `/removed`                 | Add authority to removed list (restrict)           |
| DELETE | `/removed/{authorityName}` | Remove authority from removed list (restore)       |

---

## 🚀 How to Use

### Step 1: Create Database Migration

Run these commands in PowerShell from the project root:

```powershell
# Add migration
dotnet ef migrations add AddRemovedAuthorities --project src/MyShop.Data --startup-project src/MyShop.Server

# Apply migration to database
dotnet ef database update --project src/MyShop.Data --startup-project src/MyShop.Server
```

### Step 2: Run the Application

```powershell
cd src/MyShop.Server
dotnet run
```

### Step 3: Test the Endpoints

Use the provided test file: `src/MyShop.Server/Controllers/user-authority-tests.http`

---

## 📚 Usage Examples

### Example 1: View User's Effective Authorities

**Request:**

```http
GET /api/v1/users/{userId}/authorities/effective
```

**Response:**

```json
{
  "success": true,
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "john_doe",
    "roleNames": ["Admin", "SalesAgent"],
    "allAuthoritiesFromRoles": ["ALL", "POST"],
    "removedAuthorities": ["DELETE"],
    "effectiveAuthorities": ["ALL", "POST"]
  }
}
```

### Example 2: Restrict DELETE Authority for a User

**Request:**

```http
POST /api/v1/users/{userId}/authorities/removed
Content-Type: application/json

{
  "authorityName": "DELETE",
  "reason": "Temporary restriction during audit",
  "removedBy": "admin_user"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Authority 'DELETE' removed from user successfully",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "authorityName": "DELETE",
    "reason": "Temporary restriction during audit",
    "removedAt": "2025-10-12T10:30:00Z",
    "removedBy": "admin_user"
  }
}
```

### Example 3: Check if User Has Authority

**Request:**

```http
GET /api/v1/users/{userId}/authorities/check/DELETE
```

**Response:**

```json
{
  "success": true,
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "authorityName": "DELETE",
    "hasAuthority": false,
    "reason": "User does not have 'DELETE' authority (may be removed or not granted)"
  }
}
```

### Example 4: Restore Authority

**Request:**

```http
DELETE /api/v1/users/{userId}/authorities/removed/DELETE
```

**Response:**

```json
{
  "success": true,
  "message": "Authority 'DELETE' restored to user successfully"
}
```

---

## 🔍 How Authority Resolution Works

### Logic Flow:

1. **Get all authorities from user's roles**

   ```
   User → Roles → RoleAuthorities → Authorities
   ```

2. **Get removed authorities for the user**

   ```
   User → RemovedAuthorities
   ```

3. **Calculate effective authorities**

   ```
   Effective = Role Authorities - Removed Authorities
   ```

4. **Special handling for "ALL" authority**
   - If user has "ALL" authority (and it's not removed), they have all permissions
   - "ALL" acts as a super admin privilege

### Example Scenarios:

#### Scenario A: Admin User (No Restrictions)

- **Roles**: Admin
- **Role Authorities**: ALL
- **Removed Authorities**: (none)
- **Effective Authorities**: ALL ✅
- **Can DELETE?**: YES (ALL grants everything)

#### Scenario B: Admin with DELETE Removed

- **Roles**: Admin
- **Role Authorities**: ALL
- **Removed Authorities**: ALL
- **Effective Authorities**: (none)
- **Can DELETE?**: NO ❌

#### Scenario C: SalesAgent with POST

- **Roles**: SalesAgent
- **Role Authorities**: POST
- **Removed Authorities**: (none)
- **Effective Authorities**: POST ✅
- **Can DELETE?**: NO (not granted)

#### Scenario D: Multiple Roles

- **Roles**: Admin, SalesAgent
- **Role Authorities**: ALL, POST (ALL from Admin, POST from SalesAgent)
- **Removed Authorities**: (none)
- **Effective Authorities**: ALL, POST ✅
- **Can DELETE?**: YES (ALL grants everything)

---

## 🎨 Database Schema

```
┌──────────────────┐
│      Users       │
│  id (PK)         │
│  username        │
│  email           │
│  ...             │
└──────┬───────────┘
       │
       ├──────────────────────────┐
       │                          │
       ▼                          ▼
┌──────────────┐          ┌──────────────────────┐
│  user_roles  │          │ removed_authorities  │
│  (join)      │          │  user_id (FK, PK)    │
└──────┬───────┘          │  authority_name (FK,PK)│
       │                  │  reason              │
       ▼                  │  removed_at          │
┌──────────────┐          │  removed_by          │
│    Roles     │          └──────────┬───────────┘
│  name (PK)   │                     │
└──────┬───────┘                     │
       │                             │
       ▼                             │
┌──────────────────┐                 │
│ role_authorities │                 │
│  (join)          │                 │
└──────┬───────────┘                 │
       │                             │
       ▼                             │
┌──────────────┐                     │
│ Authorities  │◄────────────────────┘
│  name (PK)   │
│  description │
└──────────────┘
```

---

## 🧪 Testing Checklist

- [ ] Create migration and update database
- [ ] Register a user with Admin role
- [ ] Check effective authorities (should have ALL)
- [ ] Remove DELETE authority
- [ ] Check effective authorities (should not have DELETE)
- [ ] Try to check DELETE authority (should return false)
- [ ] Restore DELETE authority
- [ ] Check effective authorities again (should have DELETE back)
- [ ] Test with user having multiple roles
- [ ] Test error cases (invalid authority name, duplicate removal, etc.)

---

## 🔐 Security Considerations

1. **Authorization**: Add proper authorization to endpoints

   - Only admins should be able to add/remove authorities
   - Users can view their own effective authorities

2. **Audit Trail**: The system automatically tracks:

   - When authority was removed (`RemovedAt`)
   - Who removed it (`RemovedBy`)
   - Why it was removed (`Reason`)

3. **Cascade Deletes**:

   - When a user is deleted, their removed authorities are also deleted
   - Authorities cannot be deleted if they're referenced in removed_authorities

4. **Validation**:
   - Authority name must exist in database
   - User must exist
   - Cannot add duplicate removals

---

## 📝 Next Steps

### Recommended Enhancements:

1. **Add Authorization Attributes**

   ```csharp
   [Authorize(Roles = "Admin")]
   public async Task<ActionResult> AddRemovedAuthority(...)
   ```

2. **Add Middleware for Authority Checking**

   ```csharp
   [RequireAuthority("DELETE")]
   public async Task<ActionResult> DeleteProduct(...)
   ```

3. **Add Expiration for Temporary Restrictions**

   ```csharp
   public DateTime? ExpiresAt { get; set; }
   ```

4. **Add Notification System**

   - Notify users when their authorities are changed
   - Send email alerts for critical restrictions

5. **Add Audit Log**
   - Track all authority changes
   - Create comprehensive audit trail

---

## 🐛 Troubleshooting

### Issue: Migration fails

**Solution**: Make sure you're running the command from the correct directory and that connection string is configured.

### Issue: "Authority not found" error

**Solution**: Check that the authority exists in the `Authorities` table. Seed data includes: POST, DELETE, ALL

### Issue: User still has authority after removal

**Solution**: Verify the removed authority is actually in the user's role authorities. Check the effective authorities endpoint.

### Issue: Cannot delete authority

**Solution**: This is by design (Restrict delete). Remove all references in `removed_authorities` first.

---

## 📞 Support

For questions or issues:

1. Check the test file examples
2. Review the API documentation in Swagger/OpenAPI
3. Check logs for detailed error messages

---

## ✅ Implementation Checklist

- [x] Create RemovedAuthorities entity
- [x] Update User entity with navigation property
- [x] Configure relationships in ShopContext
- [x] Create request/response DTOs
- [x] Create IUserAuthorityService interface
- [x] Implement UserAuthorityService
- [x] Create UserAuthorityController
- [x] Register services in DI container
- [x] Create comprehensive test file
- [x] Write documentation

**Next**: Create and run database migration!

```powershell
dotnet ef migrations add AddRemovedAuthorities --project src/MyShop.Data --startup-project src/MyShop.Server
dotnet ef database update --project src/MyShop.Data --startup-project src/MyShop.Server
```
