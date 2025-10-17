# RemovedAuthorities Quick Reference

## 🎯 Quick Start

### 1. Run Migration

```powershell
dotnet ef migrations add AddRemovedAuthorities --project src/MyShop.Data --startup-project src/MyShop.Server
dotnet ef database update --project src/MyShop.Data --startup-project src/MyShop.Server
```

### 2. Test Endpoints

All endpoints are in: `src/MyShop.Server/Controllers/user-authority-tests.http`

---

## 📍 API Endpoints

Base: `/api/v1/users/{userId}/authorities`

| Endpoint                   | Method | Purpose                           |
| -------------------------- | ------ | --------------------------------- |
| `/effective`               | GET    | View user's effective authorities |
| `/check/{authorityName}`   | GET    | Check if user has authority       |
| `/removed`                 | GET    | List removed authorities          |
| `/removed`                 | POST   | Restrict authority                |
| `/removed/{authorityName}` | DELETE | Restore authority                 |

---

## 💡 Common Operations

### Restrict DELETE for User

```http
POST /api/v1/users/{userId}/authorities/removed
{
  "authorityName": "DELETE",
  "reason": "Security review pending",
  "removedBy": "admin"
}
```

### Check User's Effective Authorities

```http
GET /api/v1/users/{userId}/authorities/effective
```

### Restore Authority

```http
DELETE /api/v1/users/{userId}/authorities/removed/DELETE
```

---

## 🔑 Key Concepts

**Formula:**

```
Effective Authorities = (Role Authorities) - (Removed Authorities)
```

**Special Case:**

- "ALL" authority = super admin (grants all permissions)
- Unless "ALL" itself is removed

---

## 📊 Example Workflow

1. User has role **Admin** → gets **ALL** authority
2. Admin removes **DELETE** authority → User loses DELETE
3. Check `/effective` → sees [ALL] but NOT [DELETE]
4. Check `/check/DELETE` → returns `false`
5. Admin restores **DELETE** → User gets DELETE back

---

## 🗄️ Database

**New Table:** `removed_authorities`

```sql
- user_id (FK → users.id)
- authority_name (FK → authorities.name)
- reason (text)
- removed_at (timestamp)
- removed_by (varchar)
PRIMARY KEY (user_id, authority_name)
```

---

## 📁 Files Created

### Entities

- `src/MyShop.Data/Entities/RemovedAuthorities.cs`

### DTOs

- `src/MyShop.Shared/DTOs/Requests/AddRemovedAuthorityRequest.cs`
- `src/MyShop.Shared/DTOs/Responses/AuthorityResponses.cs`

### Services

- `src/MyShop.Server/Services/Interfaces/IUserAuthorityService.cs`
- `src/MyShop.Server/Services/Implementations/UserAuthorityService.cs`

### Controllers

- `src/MyShop.Server/Controllers/UserAuthorityController.cs`

### Tests

- `src/MyShop.Server/Controllers/user-authority-tests.http`

### Docs

- `docs/REMOVED_AUTHORITIES_IMPLEMENTATION.md`

---

## ✅ Pre-flight Checklist

Before testing:

- [ ] Migration created
- [ ] Database updated
- [ ] User exists in database
- [ ] User has at least one role
- [ ] Replace `{{userId}}` in test file with real GUID

---

## 🎓 Use Cases

1. **Temporary Restrictions**: Suspend user's delete permission during audit
2. **Fine-grained Control**: User is Admin but shouldn't delete certain things
3. **Security Incidents**: Quick removal of dangerous permissions
4. **Compliance**: Remove specific permissions without changing roles
5. **Testing**: Test behavior with different permission sets

---

## 🚨 Error Handling

| Error                       | Cause                         | Solution                 |
| --------------------------- | ----------------------------- | ------------------------ |
| 404 User not found          | Invalid userId                | Check user exists        |
| 400 Authority doesn't exist | Invalid authority name        | Use: POST, DELETE, ALL   |
| 400 Already removed         | Duplicate removal             | Check removed list first |
| 404 Not in removed list     | Trying to restore non-removed | Check removed list       |

---

## 🔄 Typical Admin Workflow

```
1. GET /effective          → See current authorities
2. POST /removed           → Restrict authority
3. GET /effective          → Verify restriction
4. GET /check/{authority}  → Confirm user can't use it
5. DELETE /removed/{auth}  → Restore when needed
6. GET /effective          → Verify restoration
```

---

## 🎯 Pro Tips

1. **Always check `/effective` after changes** to verify
2. **Use meaningful `reason` field** for audit trail
3. **Record `removedBy` username** for accountability
4. **"ALL" authority is special** - it grants everything
5. **Removed authorities persist** across role changes

---

## 📞 Need Help?

Check full documentation:

- `docs/REMOVED_AUTHORITIES_IMPLEMENTATION.md`
- `docs/REMOVED_AUTHORITIES_SOLUTION.md`
