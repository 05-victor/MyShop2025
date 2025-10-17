# RemovedAuthorities System - Visual Guide

## 🎨 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                              │
│  (Browser, Postman, REST Client)                                 │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP Requests
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    CONTROLLER LAYER                              │
│                                                                  │
│  UserAuthorityController                                         │
│  ├─ GET  /authorities/effective      → Get effective authorities│
│  ├─ GET  /authorities/check/{auth}   → Check if user has auth  │
│  ├─ GET  /authorities/removed        → List removed authorities │
│  ├─ POST /authorities/removed        → Restrict authority       │
│  └─ DELETE /authorities/removed/{auth} → Restore authority      │
│                                                                  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      SERVICE LAYER                               │
│                                                                  │
│  UserAuthorityService                                            │
│  ├─ GetEffectiveAuthoritiesAsync()    → Calculate effective     │
│  ├─ HasAuthorityAsync()                → Check permission        │
│  ├─ AddRemovedAuthorityAsync()         → Add restriction        │
│  ├─ RemoveRemovedAuthorityAsync()      → Remove restriction     │
│  └─ GetRemovedAuthoritiesAsync()       → Get restrictions       │
│                                                                  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATA LAYER (EF Core)                        │
│                                                                  │
│  ShopContext                                                     │
│  ├─ Users                                                        │
│  ├─ Roles                                                        │
│  ├─ Authorities                                                  │
│  ├─ RoleAuthorities                                              │
│  └─ RemovedAuthorities        ← NEW TABLE                       │
│                                                                  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATABASE (PostgreSQL)                         │
│                                                                  │
│  Tables:                                                         │
│  • users                                                         │
│  • roles                                                         │
│  • authorities                                                   │
│  • user_roles (join table)                                       │
│  • role_authorities (join table)                                 │
│  • removed_authorities (NEW - blacklist)                         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Authority Resolution Flow

```
START: Check if User has Authority "DELETE"
   │
   ▼
┌──────────────────────────────┐
│ 1. Get User from Database    │
│    Include: Roles,            │
│    RemovedAuthorities         │
└─────────────┬────────────────┘
              │
              ▼
┌──────────────────────────────┐
│ 2. Get All Role Authorities  │
│    User → Roles →             │
│    RoleAuthorities →          │
│    ["ALL", "POST", "DELETE"]  │
└─────────────┬────────────────┘
              │
              ▼
┌──────────────────────────────┐
│ 3. Get Removed Authorities   │
│    User → RemovedAuthorities  │
│    ["DELETE"]                 │
└─────────────┬────────────────┘
              │
              ▼
┌──────────────────────────────┐
│ 4. Calculate Effective       │
│    ["ALL", "POST", "DELETE"]  │
│    MINUS                      │
│    ["DELETE"]                 │
│    = ["ALL", "POST"]          │
└─────────────┬────────────────┘
              │
              ▼
┌──────────────────────────────┐
│ 5. Check for "DELETE"        │
│    Is "DELETE" in             │
│    ["ALL", "POST"]?           │
│    → NO                       │
└─────────────┬────────────────┘
              │
              ▼
┌──────────────────────────────┐
│ 6. Check for "ALL"           │
│    Is "ALL" in                │
│    ["ALL", "POST"]?           │
│    → YES → GRANT ACCESS       │
└─────────────┬────────────────┘
              │
              ▼
         RESULT: TRUE
    (User has access via "ALL")
```

---

## 📊 Database Relationships

```
┌──────────────────┐
│      users       │
│ ┌──────────────┐ │
│ │ id (PK)      │ │◄───────┐
│ │ username     │ │        │
│ │ email        │ │        │
│ │ ...          │ │        │
│ └──────────────┘ │        │
└────┬─────────┬───┘        │
     │         │            │
     │         │            │
     │         └────────────┼────────────┐
     │                      │            │
     │                      │            │
     ▼                      ▼            │
┌─────────────┐    ┌──────────────────┐ │
│ user_roles  │    │ removed_authorities│ │
│ (join table)│    │ ┌──────────────┐ │ │
└──────┬──────┘    │ │ user_id (FK) │─┼─┘
       │           │ │ auth_name(FK)│─┼─┐
       │           │ │ reason       │ │ │
       ▼           │ │ removed_at   │ │ │
┌─────────────┐    │ │ removed_by   │ │ │
│    roles    │    │ └──────────────┘ │ │
│ ┌─────────┐ │    └──────────────────┘ │
│ │ name(PK)│ │                          │
│ └─────────┘ │                          │
└──────┬──────┘                          │
       │                                 │
       │                                 │
       ▼                                 │
┌──────────────────┐                     │
│ role_authorities │                     │
│    (join table)  │                     │
└──────┬───────────┘                     │
       │                                 │
       │                                 │
       ▼                                 │
┌──────────────┐                         │
│ authorities  │◄────────────────────────┘
│ ┌──────────┐ │
│ │ name (PK)│ │
│ │ descrip. │ │
│ └──────────┘ │
└──────────────┘
```

---

## 🎯 Example Scenarios (Visual)

### Scenario 1: Admin User (No Restrictions)

```
User: John
Role: Admin
═══════════════════════════════════════

Step 1: Get Role Authorities
   Admin → [ALL]

Step 2: Get Removed Authorities
   (empty) → []

Step 3: Calculate Effective
   [ALL] - [] = [ALL] ✅

Step 4: Check "DELETE" permission
   "ALL" grants everything → TRUE ✅

RESULT: John CAN delete
```

### Scenario 2: Admin with DELETE Restricted

```
User: John
Role: Admin
Restriction: DELETE removed
═══════════════════════════════════════

Step 1: Get Role Authorities
   Admin → [ALL]

Step 2: Get Removed Authorities
   RemovedAuthorities → [ALL]

Step 3: Calculate Effective
   [ALL] - [ALL] = [] ❌

Step 4: Check "DELETE" permission
   "DELETE" not in [] → FALSE ❌

RESULT: John CANNOT delete
```

### Scenario 3: SalesAgent (Normal)

```
User: Jane
Role: SalesAgent
═══════════════════════════════════════

Step 1: Get Role Authorities
   SalesAgent → [POST]

Step 2: Get Removed Authorities
   (empty) → []

Step 3: Calculate Effective
   [POST] - [] = [POST] ✅

Step 4: Check "DELETE" permission
   "DELETE" not in [POST] → FALSE ❌

RESULT: Jane CANNOT delete
       (not granted, not removed)
```

### Scenario 4: Multiple Roles with Restriction

```
User: Mike
Roles: Admin, SalesAgent
Restriction: DELETE removed
═══════════════════════════════════════

Step 1: Get Role Authorities
   Admin → [ALL]
   SalesAgent → [POST]
   Combined → [ALL, POST]

Step 2: Get Removed Authorities
   RemovedAuthorities → [DELETE]

Step 3: Calculate Effective
   [ALL, POST] - [DELETE] = [ALL, POST] ✅

Step 4: Check "DELETE" permission
   "DELETE" not in [ALL, POST] → FALSE
   BUT "ALL" is present → TRUE ✅

RESULT: Mike CAN delete
       (via "ALL" authority)
```

---

## 🔐 Permission Check Logic (Flowchart)

```
                    START
                      │
                      ▼
         ┌─────────────────────────┐
         │ Get Effective           │
         │ Authorities for User    │
         └────────────┬────────────┘
                      │
                      ▼
         ┌─────────────────────────┐
         │ Does list contain       │
         │ "ALL"?                  │
         └────┬────────────┬───────┘
              │            │
             YES          NO
              │            │
              ▼            ▼
    ┌──────────────┐  ┌─────────────────┐
    │ RETURN TRUE  │  │ Does list contain│
    │ (Super Admin)│  │ requested auth? │
    └──────────────┘  └────┬───────┬────┘
                           │       │
                          YES     NO
                           │       │
                           ▼       ▼
                  ┌──────────┐  ┌──────────┐
                  │ RETURN   │  │ RETURN   │
                  │ TRUE     │  │ FALSE    │
                  └──────────┘  └──────────┘
```

---

## 📝 State Transitions

```
┌─────────────────────────────────────────────────────┐
│             INITIAL STATE                           │
│                                                     │
│  User: John                                         │
│  Role: Admin                                        │
│  Role Authorities: [ALL, POST, DELETE]              │
│  Removed Authorities: []                            │
│  Effective Authorities: [ALL, POST, DELETE] ✅      │
│                                                     │
└─────────────────────┬───────────────────────────────┘
                      │
                      │ POST /authorities/removed
                      │ { "authorityName": "DELETE" }
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│            RESTRICTED STATE                         │
│                                                     │
│  User: John                                         │
│  Role: Admin                                        │
│  Role Authorities: [ALL, POST, DELETE]              │
│  Removed Authorities: [DELETE] ← ADDED              │
│  Effective Authorities: [ALL, POST] ❌ DELETE GONE │
│                                                     │
└─────────────────────┬───────────────────────────────┘
                      │
                      │ DELETE /authorities/removed/DELETE
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│            RESTORED STATE                           │
│                                                     │
│  User: John                                         │
│  Role: Admin                                        │
│  Role Authorities: [ALL, POST, DELETE]              │
│  Removed Authorities: [] ← CLEARED                  │
│  Effective Authorities: [ALL, POST, DELETE] ✅      │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## 🎯 API Request/Response Flow

```
CLIENT                    CONTROLLER              SERVICE                DATABASE
  │                          │                      │                      │
  │  GET /effective          │                      │                      │
  ├─────────────────────────►│                      │                      │
  │                          │  GetEffectiveAuthsAsync()                   │
  │                          ├─────────────────────►│                      │
  │                          │                      │  Query User+Roles    │
  │                          │                      ├─────────────────────►│
  │                          │                      │◄─────────────────────┤
  │                          │                      │  Query RemovedAuths  │
  │                          │                      ├─────────────────────►│
  │                          │                      │◄─────────────────────┤
  │                          │                      │                      │
  │                          │                      │  Calculate:          │
  │                          │                      │  Role Auths -        │
  │                          │                      │  Removed Auths       │
  │                          │                      │                      │
  │                          │  ◄────EffectiveResponse                     │
  │                          │◄─────────────────────┤                      │
  │  ◄────200 OK + Data      │                      │                      │
  │◄─────────────────────────┤                      │                      │
  │                          │                      │                      │
```

---

## 💡 Decision Tree: When to Use

```
                     ┌──────────────────┐
                     │ Need to restrict │
                     │ user authority?  │
                     └────────┬─────────┘
                              │
                     ┌────────▼─────────┐
                     │ Is it permanent? │
                     └────┬────────┬────┘
                          │        │
                         YES      NO
                          │        │
                          ▼        ▼
                  ┌───────────┐  ┌──────────────┐
                  │ Change    │  │ Use          │
                  │ User's    │  │ RemovedAuths │
                  │ Role      │  │ (temporary)  │
                  └───────────┘  └──────────────┘
                                        │
                              ┌─────────▼──────────┐
                              │ Will it affect     │
                              │ many users?        │
                              └────┬──────────┬────┘
                                   │          │
                                  YES        NO
                                   │          │
                                   ▼          ▼
                          ┌──────────────┐  ┌──────────────┐
                          │ Modify Role  │  │ Use          │
                          │ Authorities  │  │ RemovedAuths │
                          └──────────────┘  │ (individual) │
                                            └──────────────┘
```

---

This visual guide shows the complete architecture and flow of the RemovedAuthorities system!
