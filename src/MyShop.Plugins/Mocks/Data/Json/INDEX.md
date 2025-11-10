# 📊 Mock Data Index - MyShop 2025

## 🎯 Tổng quan

Thư mục này chứa **9 file JSON mock data** được tạo để hỗ trợ phát triển WinUI frontend mà không cần backend API.

---

## 📂 Danh sách Files

| # | File | Records | Mục đích | Priority |
|---|------|---------|----------|----------|
| 1 | [auth.json](auth.json) | 5 users, 3 codes | Authentication & Admin Codes | 🔴 Gấp |
| 2 | [profiles.json](profiles.json) | 5 profiles | User Profile Information | 🟡 Cao |
| 3 | [categories.json](categories.json) | 8 categories | Product Categories | 🟡 Cao |
| 4 | [products.json](products.json) | 10 products | Product Catalog | 🔴 Gấp |
| 5 | [orders.json](orders.json) | 8 orders, 12 items | Order Management | 🔴 Gấp |
| 6 | [dashboard.json](dashboard.json) | Summary + Charts | Dashboard Analytics | 🔴 Gấp |
| 7 | [reports.json](reports.json) | Sales + Revenue | Business Reports | 🟢 TB |
| 8 | [settings.json](settings.json) | App/System/Business | Application Settings | 🟢 TB |
| 9 | [customers.json](customers.json) | 8 customers | Customer Management | 🟢 TB |

---

## 🚀 Quick Start

### 1. Đọc tài liệu chi tiết
```
📖 README.md - Tài liệu đầy đủ về cấu trúc và use cases
```

### 2. Test dữ liệu
```bash
# Xem nội dung file (PowerShell)
Get-Content auth.json | ConvertFrom-Json | ConvertTo-Json -Depth 10

# Or using VSCode
# Click vào file → VSCode JSON viewer
```

### 3. Tạo Mock Repository (Next Step)
```csharp
// src/MyShop.Plugins/Mocks/Repositories/MockProductRepository.cs
public class MockProductRepository : IProductRepository
{
    private readonly List<ProductResponse> _products;
    
    public MockProductRepository()
    {
        // Load products.json
        var json = File.ReadAllText("Mocks/Data/Json/products.json");
        var data = JsonSerializer.Deserialize<ProductData>(json);
        _products = data.Products;
    }
    
    public async Task<Result<List<ProductResponse>>> GetAllAsync()
    {
        await Task.Delay(300); // Simulate network
        return Result<List<ProductResponse>>.Success(_products);
    }
}
```

---

## 📊 Data Statistics Summary

### Users & Authentication
- **5 Users**: 1 Admin, 1 Sales Agent, 3 Customers
- **3 Admin Codes**: 2 active, 1 expired
- **Trial Users**: 1 active (customer1)
- **Unverified**: 1 user (newuser)

### Product Catalog
- **10 Products**: Across 5 categories in dataset
- **8 Categories**: Smartphones, Laptops, Tablets, etc.
- **Low Stock**: 1 product (Logitech MX Master - 3 units)
- **Out of Stock**: 1 product (Nintendo Switch - 0 units)

### Orders & Revenue
- **8 Orders**: 5 paid, 2 created, 1 cancelled
- **12 Order Items**: Average 1.5 items/order
- **Total Revenue**: 329.95M VND
- **Total Profit**: 60.15M VND
- **Profit Margin**: 18.2%

### Customers
- **8 Customers**: 3 VIP, 5 Regular
- **Top Spender**: Bùi Văn Hùng (80M VND)

---

## 🔗 API Contract Mapping

| JSON File | API Endpoints | Status |
|-----------|---------------|--------|
| auth.json | POST /api/v1/auth/login<br>POST /api/v1/auth/register<br>POST /api/v1/users/verify-admin-code | ✅ Done |
| profiles.json | GET /api/v1/profiles/get<br>PUT /api/v1/profiles/update | ⏳ TODO |
| categories.json | GET /api/v1/categories<br>POST /api/v1/categories | ⏳ TODO |
| products.json | GET /api/v1/products<br>POST /api/v1/products<br>GET /api/v1/products/low-stock | ⏳ TODO |
| orders.json | GET /api/v1/orders<br>POST /api/v1/orders<br>GET /api/v1/orders/top-revenue-products | ⏳ TODO |
| dashboard.json | GET /api/v1/dashboard/summary<br>GET /api/v1/dashboard/revenue-chart | ⏳ TODO |
| reports.json | GET /api/v1/reports/sales<br>GET /api/v1/reports/revenue | ⏳ TODO |
| settings.json | GET /api/v1/settings/app<br>PUT /api/v1/settings/app | ⏳ TODO |
| customers.json | GET /api/v1/customers | ⏳ TODO |

---

## 🎨 Features Demonstrated

### ✅ Authentication & Authorization
- Multi-role system (ADMIN, SALESAGENT, USER)
- Trial activation workflow
- Admin code verification
- Email verification status

### ✅ Product Management
- Full product catalog với detailed info
- Category organization
- Stock management (low stock, out of stock)
- Pricing (import vs selling)
- Commission calculations
- Rating system

### ✅ Order Processing
- Order lifecycle (CREATED → PAID/CANCELLED)
- Multi-item orders
- Discount handling
- Sales agent tracking
- Customer information

### ✅ Business Analytics
- Dashboard KPIs
- Revenue charts (daily/weekly/monthly/yearly)
- Top selling products
- Low stock alerts
- Sales by category
- Profit margin analysis

### ✅ Reporting
- Sales reports by product/date/category
- Revenue breakdown
- Profit analysis
- Top revenue products ranking

### ✅ Settings & Configuration
- User preferences (theme, language, page size)
- System configuration
- Business information
- Notification settings

### ✅ Customer Management
- Customer profiles
- Purchase history
- VIP customer identification
- Contact information

---

## 🧪 Testing Scenarios

### Login Testing
```json
// Test credentials (auth.json)
Admin:        admin / admin123
Sales Agent:  salesagent1 / sales123
Customer:     customer1 / customer123
Trial User:   customer1 (active trial until 11/10/2025)
Unverified:   newuser / newuser123
```

### Product Scenarios
- View all products → 10 items
- Filter low stock → Logitech MX Master (3 units)
- Filter out of stock → Nintendo Switch (0 units)
- Sort by rating → Sony WH-1000XM5 (4.9⭐)
- Filter by category → Smartphones (2 items in mock)

### Order Scenarios
- Create new order → Status: CREATED
- Mark as paid → Update status to PAID
- Cancel order → Set CANCELLED with reason
- View agent orders → Filter by salesAgentId

### Dashboard Testing
- Today revenue → 56.46M (from 2 orders)
- Week revenue → 229.95M
- Month revenue → 329.95M
- Low stock alerts → 3 products
- Top selling → iPhone 15 Pro (3 sold)

---

## 📋 Validation Checklist

- [x] All JSON files are valid format
- [x] GUIDs are unique and consistent
- [x] Foreign keys match across files
- [x] Dates are chronologically correct
- [x] Prices: importPrice < sellingPrice
- [x] Stock quantities realistic
- [x] Order totals calculated correctly
- [x] Profit margins accurate
- [x] Vietnamese phone format (09xx/08xx)
- [x] Email formats valid
- [x] Trial dates within 15 days
- [x] Revenue chart data consistent

---

## 🔄 Next Steps

### Phase 1: Mock Repository Implementation
1. Create `MockProductRepository.cs`
2. Create `MockOrderRepository.cs`
3. Create `MockCategoryRepository.cs`
4. Create `MockDashboardRepository.cs`
5. Create `MockReportRepository.cs`

### Phase 2: Dependency Injection
1. Register repositories in `Bootstrapper.cs`
2. Configure services in `App.xaml.cs`
3. Add feature flags (UseMockData = true/false)

### Phase 3: ViewModel Integration
1. Update `ProductsViewModel` to use IProductRepository
2. Update `OrdersViewModel` to use IOrderRepository
3. Update `DashboardViewModel` to use IDashboardRepository
4. Add error handling and loading states

### Phase 4: UI Testing
1. Test Products page with mock data
2. Test Orders page with mock data
3. Test Dashboard with charts
4. Test Reports with filters
5. Test Settings page

---

## 📚 Documentation References

- **Full Documentation**: [README.md](README.md)
- **API Contract**: `/docs-temp/structure/api/00-FRONTEND-API-REQUIREMENTS.md`
- **Mock Guide**: `/docs-temp/mock-docs/MOCK-DATA-GUIDE.md`
- **DTOs**: `/src/MyShop.Shared/DTOs/Responses/`
- **Existing Mock**: `/src/MyShop.Plugins/Mocks/Data/MockAuthData.cs`

---

## 🆘 Troubleshooting

### Issue: JSON parse error
**Solution**: Validate JSON at https://jsonlint.com

### Issue: GUID not found
**Solution**: Check GUID consistency across files using search

### Issue: Date format error
**Solution**: All dates use ISO 8601 format (YYYY-MM-DDTHH:mm:ssZ)

### Issue: Missing fields
**Solution**: Compare with DTOs in `/src/MyShop.Shared/DTOs/Responses/`

---

## 📞 Support

**Questions?** Check [README.md](README.md) first!

**Issues?** Review API contract and DTOs for correct field names.

---

**📅 Created:** November 10, 2025  
**👤 Author:** AI Assistant  
**📦 Version:** 1.0.0  
**🎯 Status:** ✅ Complete - Ready for Repository Implementation
