# Solution: User Authority Restrictions

## Problem

You want to restrict specific authorities for individual users, even though they inherit those authorities from their roles.

## Current Architecture

- **User** → has many → **Roles**
- **Role** → has many → **Authorities** (via RoleAuthorities join table)
- **Result**: User gets all authorities from all their roles

## Proposed Solution: RemovedAuthorities Table

### Concept

Create a `RemovedAuthorities` table that acts as a "blacklist" or "exclusion list" to remove specific authorities from specific users, overriding the authorities they would normally inherit from their roles.

### Authority Resolution Logic

```
User's Effective Authorities =
  (All Authorities from User's Roles) - (Authorities in RemovedAuthorities for that User)
```

### Example Scenario

1. **User "John"** has role **"Admin"**
2. **Admin role** grants authorities: **[ALL, POST, DELETE]**
3. **RemovedAuthorities** for John: **[DELETE]**
4. **John's Effective Authorities**: **[ALL, POST]** ✅

---

## Implementation Plan

### 1. Create Entity: `RemovedAuthorities.cs`

```csharp
public class RemovedAuthorities
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string AuthorityName { get; set; } = string.Empty;
    public Authority Authority { get; set; } = null!;

    // Optional: Track why authority was removed
    public string? Reason { get; set; }
    public DateTime RemovedAt { get; set; } = DateTime.UtcNow;
    public string? RemovedBy { get; set; } // Admin who removed it
}
```

### 2. Update User Entity

Add navigation property:

```csharp
public ICollection<RemovedAuthorities> RemovedAuthorities { get; set; } = new List<RemovedAuthorities>();
```

### 3. Update ShopContext

```csharp
public DbSet<RemovedAuthorities> RemovedAuthorities { get; set; }

// In OnModelCreating:
modelBuilder.Entity<RemovedAuthorities>()
    .HasKey(ra => new { ra.UserId, ra.AuthorityName });

modelBuilder.Entity<RemovedAuthorities>()
    .HasOne(ra => ra.User)
    .WithMany(u => u.RemovedAuthorities)
    .HasForeignKey(ra => ra.UserId);

modelBuilder.Entity<RemovedAuthorities>()
    .HasOne(ra => ra.Authority)
    .WithMany()
    .HasForeignKey(ra => ra.AuthorityName);

modelBuilder.Entity<RemovedAuthorities>()
    .ToTable("removed_authorities");
```

### 4. Create Service to Get User's Effective Authorities

```csharp
public interface IUserAuthorityService
{
    Task<IEnumerable<string>> GetEffectiveAuthoritiesAsync(Guid userId);
    Task<bool> HasAuthorityAsync(Guid userId, string authorityName);
    Task AddRemovedAuthorityAsync(Guid userId, string authorityName, string? reason = null);
    Task RemoveRemovedAuthorityAsync(Guid userId, string authorityName);
}

public class UserAuthorityService : IUserAuthorityService
{
    private readonly ShopContext _context;

    public async Task<IEnumerable<string>> GetEffectiveAuthoritiesAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.RoleAuthorities)
                    .ThenInclude(ra => ra.Authority)
            .Include(u => u.RemovedAuthorities)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return Enumerable.Empty<string>();

        // Get all authorities from roles
        var roleAuthorities = user.Roles
            .SelectMany(r => r.RoleAuthorities)
            .Select(ra => ra.AuthorityName)
            .Distinct();

        // Remove blacklisted authorities
        var removedAuthorities = user.RemovedAuthorities
            .Select(ra => ra.AuthorityName)
            .ToHashSet();

        return roleAuthorities
            .Where(auth => !removedAuthorities.Contains(auth))
            .ToList();
    }

    public async Task<bool> HasAuthorityAsync(Guid userId, string authorityName)
    {
        var authorities = await GetEffectiveAuthoritiesAsync(userId);

        // Check for "ALL" authority (super admin)
        if (authorities.Contains("ALL"))
            return true;

        return authorities.Contains(authorityName);
    }
}
```

---

## Alternative Approaches

### Option 2: User-Specific Authority Grants (Additive)

Instead of removing, you could add a `UserAuthorities` table that grants additional authorities:

```
User's Authorities = (Role Authorities) + (User-Specific Authorities)
```

**Pros:** More flexible, allows granting without role changes
**Cons:** Harder to track, can lead to permission sprawl

### Option 3: User-Specific Role Overrides

Create a complete override system:

```csharp
public class UserAuthorityOverride
{
    public Guid UserId { get; set; }
    public string AuthorityName { get; set; }
    public bool IsGranted { get; set; } // true = add, false = remove
}
```

**Pros:** Most flexible
**Cons:** Most complex

---

## Recommended: Hybrid Approach

I recommend using **RemovedAuthorities** (blacklist) as your primary solution because:

1. ✅ **Role-based permissions remain primary** - Most users get permissions from roles
2. ✅ **Exceptions are visible** - Easy to see who has restricted permissions
3. ✅ **Audit trail** - Track when/why/who removed authorities
4. ✅ **Security-first** - Explicit removals are safer than implicit grants
5. ✅ **Simple to understand** - Clear logic: Role permissions minus removals

---

## Migration Strategy

### Phase 1: Create Entity and Migration

1. Create `RemovedAuthorities` entity
2. Update `User` entity with navigation property
3. Update `ShopContext` configuration
4. Create and run migration

### Phase 2: Create Service Layer

1. Create `IUserAuthorityService` interface
2. Implement `UserAuthorityService`
3. Register in DI container

### Phase 3: Create API Endpoints

```
POST   /api/v1/users/{userId}/removed-authorities     # Add restriction
DELETE /api/v1/users/{userId}/removed-authorities/{authorityName}  # Remove restriction
GET    /api/v1/users/{userId}/effective-authorities   # Get effective authorities
GET    /api/v1/users/{userId}/removed-authorities     # Get restrictions
```

### Phase 4: Update Authorization Logic

Use `HasAuthorityAsync()` in your authorization middleware/attributes

---

## Usage Examples

### Example 1: Restrict DELETE for specific admin

```csharp
await _userAuthorityService.AddRemovedAuthorityAsync(
    userId: adminUserId,
    authorityName: "DELETE",
    reason: "Temporary restriction during audit period"
);
```

### Example 2: Check if user can delete

```csharp
bool canDelete = await _userAuthorityService.HasAuthorityAsync(userId, "DELETE");
if (!canDelete)
{
    return Forbidden("You don't have permission to delete");
}
```

### Example 3: View user's effective permissions

```csharp
var effectiveAuthorities = await _userAuthorityService.GetEffectiveAuthoritiesAsync(userId);
// Returns: ["POST", "ALL"] but not "DELETE" if it was removed
```

---

## Database Schema Visualization

```
┌──────────┐         ┌───────────────┐         ┌──────────┐
│  Users   │────────>│  user_roles   │<────────│  Roles   │
└──────────┘         └───────────────┘         └──────────┘
     │                                                │
     │                                                │
     │                                                v
     │                                          ┌──────────────────┐
     │                                          │ role_authorities │
     │                                          └──────────────────┘
     │                                                │
     │                                                v
     │                                          ┌─────────────┐
     │                                          │ Authorities │
     │                                          └─────────────┘
     │                                                ^
     │                                                │
     v                                                │
┌────────────────────┐                               │
│removed_authorities │───────────────────────────────┘
└────────────────────┘
   (Blacklist)
```

---

## Would you like me to implement this solution?

I can create:

1. ✅ `RemovedAuthorities` entity
2. ✅ Updated `User` entity
3. ✅ Updated `ShopContext` with relationships
4. ✅ `IUserAuthorityService` and implementation
5. ✅ API Controller with endpoints
6. ✅ DTOs for requests/responses
7. ✅ Database migration file
8. ✅ Test file with examples

Let me know if you want me to proceed with the implementation!
