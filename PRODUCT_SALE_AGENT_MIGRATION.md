# Add SaleAgent to Product - Migration Guide

## Overview
This migration adds a `SaleAgentId` column to the `products` table to track which user (sale agent) published each product.

## Migration Commands

### Create Migration
```bash
dotnet ef migrations add AddSaleAgentToProduct --project src/MyShop.Data --startup-project src/MyShop.Server
```

### Apply Migration
```bash
dotnet ef database update --project src/MyShop.Data --startup-project src/MyShop.Server
```

### Rollback (if needed)
```bash
dotnet ef database update <PreviousMigrationName> --project src/MyShop.Data --startup-project src/MyShop.Server
```

## Database Changes

### New Column
- **Table**: `products`
- **Column**: `sale_agent_id`
- **Type**: `uuid` (Guid)
- **Nullable**: `true` (optional)
- **Foreign Key**: References `users(id)`

### SQL Preview (PostgreSQL)
```sql
ALTER TABLE products 
ADD COLUMN sale_agent_id uuid NULL;

ALTER TABLE products
ADD CONSTRAINT FK_products_users_sale_agent_id 
FOREIGN KEY (sale_agent_id) 
REFERENCES users(id) 
ON DELETE SET NULL;

CREATE INDEX IX_products_sale_agent_id 
ON products(sale_agent_id);
```

## Features

### Automatic Sale Agent Assignment
When creating a product:
- If `SaleAgentId` is not provided in the request, the system automatically assigns the current authenticated user as the sale agent
- This ensures products are always associated with a user (if authenticated)

### Product Response Enhancement
Product responses now include:
- `SaleAgentId`: The GUID of the sale agent
- `SaleAgentUsername`: The username of the sale agent
- `SaleAgentFullName`: The full name of the sale agent (from profile)

## API Changes

### CreateProductRequest
```json
{
  "SKU": "PROD-001",
  "Name": "Sample Product",
  // ... other fields
  "CategoryId": "guid-here",
  "SaleAgentId": "guid-here"  // ? NEW: Optional, auto-assigned if omitted
}
```

### UpdateProductRequest
```json
{
  "Name": "Updated Product",
  // ... other fields
  "SaleAgentId": "new-guid-here"  // ? NEW: Optional, can reassign sale agent
}
```

### ProductResponse
```json
{
  "Id": "guid",
  "Name": "Product Name",
  // ... other fields
  "CategoryName": "Electronics",
  "SaleAgentId": "guid",           // ? NEW
  "SaleAgentUsername": "john_doe",  // ? NEW
  "SaleAgentFullName": "John Doe"   // ? NEW
}
```

## Use Cases

1. **Product Creation by Authenticated User**
   - User creates a product ? Automatically assigned as sale agent
   - Admin creates product for another agent ? Can specify `SaleAgentId`

2. **Product Listing**
   - View all products with their sale agents
   - Filter products by sale agent (can be added later)

3. **Commission Tracking**
   - Products have `CommissionRate` field
   - `SaleAgentId` links to the agent who should receive commission
   - Perfect for calculating sales commissions per agent

4. **Sales Analytics**
   - Track which agents publish the most products
   - Monitor product performance by agent
   - Calculate sales and commissions per agent

## Testing

### Test Scenarios

1. **Create Product (Authenticated User)**
```http
POST /api/v1/products
Authorization: Bearer <token>
Content-Type: application/json

{
  "SKU": "TEST-001",
  "Name": "Test Product",
  "ImportPrice": 100,
  "SellingPrice": 150,
  "Quantity": 10,
  "CommissionRate": 0.1,
  "CategoryId": "<valid-category-id>"
  // SaleAgentId omitted - should auto-assign current user
}
```

2. **Create Product with Specific Agent (Admin)**
```http
POST /api/v1/products
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  // ... other fields
  "SaleAgentId": "<specific-user-id>"
}
```

3. **Update Sale Agent**
```http
PATCH /api/v1/products/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "SaleAgentId": "<new-agent-id>"
}
```

4. **Get Product with Sale Agent Info**
```http
GET /api/v1/products/{id}
Authorization: Bearer <token>

Response:
{
  "id": "...",
  "name": "Product Name",
  // ... other fields
  "saleAgentId": "...",
  "saleAgentUsername": "agent_username",
  "saleAgentFullName": "Agent Full Name"
}
```

## Notes

- **Nullable Foreign Key**: `SaleAgentId` is nullable to support products that may exist without a sale agent (legacy data or system-created products)
- **Cascade Delete**: If a user is deleted, their `sale_agent_id` in products will be set to NULL (not cascade delete)
- **Performance**: Index is created on `sale_agent_id` for efficient querying
- **Navigation Property**: EF Core includes `SaleAgent` navigation property for easy data loading

## Files Modified

### Entity
- ? `src/MyShop.Data/Entities/Product.cs`

### DTOs
- ? `src/MyShop.Shared/DTOs/Requests/CreateProductRequest.cs`
- ? `src/MyShop.Shared/DTOs/Requests/UpdateProductRequest.cs`
- ? `src/MyShop.Shared/DTOs/Responses/ProductResponse.cs`

### Repository
- ? `src/MyShop.Data/Repositories/Implementations/ProductRepository.cs`

### Service
- ? `src/MyShop.Server/Services/Implementations/ProductService.cs`

### Factory
- ? `src/MyShop.Server/Factories/Implementations/ProductFactory.cs`

### Mapper
- ? `src/MyShop.Server/Mappings/ProductMapper.cs`

## Next Steps

1. Run the migration command
2. Test product creation with authenticated users
3. Verify sale agent information appears in responses
4. (Optional) Add filtering/querying by sale agent
5. (Optional) Add commission calculation endpoints

---

**Migration Status**: ? Ready to run
**Breaking Changes**: ? None (backward compatible - SaleAgentId is optional)
**Data Migration Needed**: ? No (existing products will have NULL sale agent)
