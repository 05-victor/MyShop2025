# 📊 Mock Data JSON Files - MyShop 2025

## 📁 Tổng quan cấu trúc

```
src/MyShop.Plugins/Mocks/Data/Json/
├── auth.json           ✅ Users & Admin Codes
├── profiles.json       ✅ User Profiles
├── categories.json     ✅ Product Categories
├── products.json       ✅ Products
├── orders.json         ✅ Orders & Order Items
├── dashboard.json      ✅ Dashboard Summary & Charts
├── reports.json        ✅ Sales & Revenue Reports
├── settings.json       ✅ App & System Settings
└── customers.json      ✅ Customer Management
```

---

## 📋 Chi tiết từng file

### 1. **auth.json** - Authentication & Authorization

**Mục đích:** Mock data cho đăng nhập, đăng ký, và admin codes

**Nội dung:**
- ✅ **5 users** (Admin, Sales Agent, 2 Customers, 1 New User)
- ✅ **3 admin codes** (2 active, 1 expired)

**Dữ liệu mẫu:**

| Username | Email | Password | Role | Trial Active | Email Verified |
|----------|-------|----------|------|--------------|----------------|
| admin | admin@myshop.com | admin123 | ADMIN, USER | ❌ | ✅ |
| salesagent1 | sales1@myshop.com | sales123 | SALESAGENT, USER | ❌ | ✅ |
| customer1 | john.doe@gmail.com | customer123 | USER | ✅ (đến 11/10/2025) | ✅ |
| customer2 | jane.smith@gmail.com | customer456 | USER | ❌ (hết hạn) | ✅ |
| newuser | newuser@example.com | newuser123 | USER | ❌ | ❌ |

**Admin Codes:**
- `ADMIN2025-MYSHOP-001` → Active, expires 31/12/2026
- `ADMIN2025-MYSHOP-002` → Active, expires 31/12/2026
- `ADMIN2025-EXPIRED` → Deactivated (example)

**Use Cases:**
- Login testing với các roles khác nhau
- Trial activation workflow (customer1 đang trial)
- Email verification workflow (newuser chưa verify)
- Admin code verification

---

### 2. **profiles.json** - User Profiles

**Mục đích:** Thông tin chi tiết profile của users

**Nội dung:**
- ✅ **5 profiles** tương ứng với 5 users trong auth.json

**Fields:**
- userId, avatar (UI Avatars API), fullName, phoneNumber, email
- address, jobTitle, createdAt, updatedAt

**Highlights:**
- Admin: "Nguyễn Văn Admin" - System Administrator
- Sales Agent: "Trần Thị Bích" - Sales Agent
- Customers: John Doe, Jane Smith (có đầy đủ thông tin)
- New User: Chưa có profile (null fields)

**Use Cases:**
- Profile page display
- Update profile workflow
- Avatar upload placeholder (UI Avatars)

---

### 3. **categories.json** - Product Categories

**Mục đích:** Danh mục sản phẩm cho filtering và organization

**Nội dung:**
- ✅ **8 categories** phổ biến

**Categories:**
1. **Smartphones** (15 products) - Điện thoại thông minh
2. **Tablets** (8 products) - Máy tính bảng
3. **Laptops** (12 products) - Laptop gaming, văn phòng
4. **Accessories** (25 products) - Tai nghe, sạc, chuột, bàn phím
5. **Wearables** (7 products) - Apple Watch, Galaxy Watch
6. **Gaming Consoles** (5 products) - PS5, Xbox, Switch
7. **Smart Home** (10 products) - Camera, chuông cửa
8. **Audio** (18 products) - Loa, tai nghe, soundbar

**Use Cases:**
- Category dropdown filter trong Products page
- Category management (CRUD)
- Dashboard "Sales by Category"

---

### 4. **products.json** - Products Catalog

**Mục đích:** Danh mục sản phẩm với thông tin chi tiết

**Nội dung:**
- ✅ **10 products** đa dạng (Apple, Samsung, Sony, Dell, Logitech, Nintendo)

**Featured Products:**

| Product | Category | Import Price | Selling Price | Qty | Status | Rating |
|---------|----------|--------------|---------------|-----|--------|--------|
| iPhone 15 Pro 256GB | Smartphones | 25M | 29.99M | 45 | AVAILABLE | 4.8 ⭐ |
| Samsung S23 Ultra | Smartphones | 27M | 31.99M | 32 | AVAILABLE | 4.7 ⭐ |
| AirPods Pro 2 | Accessories | 5M | 6.49M | 120 | AVAILABLE | 4.9 ⭐ |
| MacBook Air M3 | Laptops | 26M | 30.99M | 28 | AVAILABLE | 4.6 ⭐ |
| iPad Pro 11 M4 | Tablets | 22M | 26.99M | 18 | AVAILABLE | 4.8 ⭐ |
| Apple Watch S9 | Wearables | 9M | 11.49M | 55 | AVAILABLE | 4.7 ⭐ |
| Sony WH-1000XM5 | Audio | 7M | 8.99M | 8 | AVAILABLE | 4.9 ⭐ |
| Dell XPS 13 | Laptops | 32M | 38.99M | 5 | AVAILABLE | 4.5 ⭐ |
| Logitech MX Master 3S | Accessories | 1.8M | 2.49M | **3** | **LOW_STOCK** | 4.8 ⭐ |
| Nintendo Switch OLED | Gaming | 7.5M | 9.49M | **0** | **OUT_OF_STOCK** | 4.6 ⭐ |

**Special Cases:**
- ⚠️ **Low Stock**: Logitech MX Master (3 units)
- 🚫 **Out of Stock**: Nintendo Switch OLED (0 units)

**Fields:**
- id, sku, name, manufacturer, deviceType
- importPrice, sellingPrice, quantity, commissionRate
- status (AVAILABLE/LOW_STOCK/OUT_OF_STOCK)
- description, imageUrl, categoryName
- rating, ratingCount, createdAt, updatedAt

**Use Cases:**
- Products list page with filters
- Low stock alerts (dashboard)
- Top selling products (dashboard)
- Product CRUD operations
- Commission calculations for sales agents

---

### 5. **orders.json** - Orders & Order Items

**Mục đích:** Mock orders với các trạng thái khác nhau

**Nội dung:**
- ✅ **8 orders** (5 PAID, 2 CREATED, 1 CANCELLED)
- ✅ **12 order items** tổng cộng

**Orders Summary:**

| Order ID | Date | Customer | Items | Total | Discount | Final Price | Status | Agent |
|----------|------|----------|-------|-------|----------|-------------|--------|-------|
| ...001 | 11/05 | Nguyễn Văn An | 1 | 29.99M | 0 | 29.99M | ✅ PAID | Trần Thị Bích |
| ...002 | 11/07 | Lê Thị Mai | 2 | 38.48M | 500K | **37.98M** | ✅ PAID | Trần Thị Bích |
| ...003 | 11/08 | Phạm Minh Tuấn | 2 | 33.48M | 0 | 33.48M | ⏳ CREATED | Trần Thị Bích |
| ...004 | 11/08 | Hoàng Văn Dũng | 1 | 22.98M | 0 | 22.98M | ⏳ CREATED | Trần Thị Bích |
| ...005 | 11/03 | Đỗ Thị Lan | 1 | 26.99M | 0 | 26.99M | ❌ CANCELLED | Trần Thị Bích |
| ...006 | 11/06 | Vũ Minh Khang | 2 | 21.97M | 970K | **21M** | ✅ PAID | Trần Thị Bích |
| ...007 | 11/02 | Trương Thị Hồng | 1 | 38.99M | 0 | 38.99M | ✅ PAID | Trần Thị Bích |
| ...008 | 10/30 | Bùi Văn Hùng | 2 | 82.96M | 2.96M | **80M** | ✅ PAID | Trần Thị Bích |

**Order Items Examples:**
- Order ...001: 1x iPhone 15 Pro → 29.99M
- Order ...002: 1x Samsung S23 Ultra + 1x AirPods Pro → 37.98M (VIP discount)
- Order ...008: 2x iPhone 15 Pro + 2x Apple Watch S9 → 80M (bulk discount)

**Fields:**
- id, orderDate, status, customerName, customerPhone, customerAddress
- salesAgentId, salesAgentName
- items[] (productId, productName, quantity, unitPrice, totalPrice)
- subtotal, discount, finalPrice, notes
- createdAt, updatedAt, paidAt, cancelledAt, cancelReason

**Use Cases:**
- Orders list page với filters (date, status, product, category)
- Sales agent performance tracking
- Revenue calculations
- Order CRUD workflows
- Discount/promotion handling

---

### 6. **dashboard.json** - Dashboard Data

**Mục đích:** Tổng hợp dữ liệu cho Admin Dashboard

**Nội dung:**
- ✅ **Dashboard Summary**: Stats tổng quan
- ✅ **Revenue Chart**: Daily/Weekly/Monthly/Yearly data

**Dashboard Summary:**
```json
{
  "date": "2025-11-08",
  "totalProducts": 10,
  "todayOrders": 2,
  "todayRevenue": 56.46M,
  "weekRevenue": 229.95M,
  "monthRevenue": 329.95M
}
```

**Low Stock Products (Top 3):**
1. Logitech MX Master 3S (3 units)
2. Dell XPS 13 (5 units)
3. Sony WH-1000XM5 (8 units)

**Top Selling Products (Top 5):**
1. iPhone 15 Pro - 3 sold, 89.97M revenue
2. AirPods Pro - 3 sold, 19.47M revenue
3. Apple Watch S9 - 4 sold, 45.96M revenue
4. Dell XPS 13 - 1 sold, 38.99M revenue
5. Samsung S23 Ultra - 1 sold, 31.99M revenue

**Recent Orders (Top 5):**
- Latest orders sorted by date descending

**Sales by Category:**
- Smartphones: 121.96M (37.0%)
- Wearables: 45.96M (13.9%)
- Laptops: 72.47M (22.0%)
- Accessories: 22.46M (6.8%)
- Audio: 8.99M (2.7%)

**Revenue Chart Data:**
- **Daily**: Last 8 days (11/01 - 11/08)
- **Weekly**: Last 5 weeks (Week 40-44)
- **Monthly**: Last 5 months (Jul-Nov 2025)
- **Yearly**: 2024-2025

**Use Cases:**
- Main dashboard page display
- KPI cards (total products, today orders, revenue)
- Charts visualization (WinUI/WPF charting)
- Alerts for low stock
- Quick insights

---

### 7. **reports.json** - Sales & Revenue Reports

**Mục đích:** Báo cáo chi tiết về doanh thu và lợi nhuận

**Nội dung:**
- ✅ **Sales Reports**: 11 records chi tiết theo product & date
- ✅ **Revenue Reports**: Summary + Breakdown + By Category
- ✅ **Top Revenue Products**: Ranked list with pagination

**Sales Reports Sample:**

| Period | Product | Category | Sold Qty | Revenue | Cost | Profit | Margin |
|--------|---------|----------|----------|---------|------|--------|--------|
| 11/08 | MacBook Air M3 | Laptops | 1 | 30.99M | 26M | 4.99M | 16.1% |
| 11/08 | Logitech MX Master | Accessories | 1 | 2.49M | 1.8M | 690K | 27.7% |
| 11/07 | Samsung S23 Ultra | Smartphones | 1 | 31.99M | 27M | 4.99M | 15.6% |

**Revenue Summary:**
- Total Revenue: **329.95M**
- Total Cost: **269.8M**
- Total Profit: **60.15M**
- Profit Margin: **18.2%**
- Order Count: 7
- Average Order Value: **47.14M**

**Revenue by Category:**

| Category | Revenue | Cost | Profit | Margin | Orders |
|----------|---------|------|--------|--------|--------|
| Smartphones | 121.96M | 102M | 19.96M | 16.4% | 4 |
| Laptops | 69.98M | 58M | 11.98M | 17.1% | 2 |
| Wearables | 45.96M | 36M | 9.96M | 21.7% | 2 |
| Accessories | 21.96M | 16.8M | 5.16M | 23.5% | 4 |
| Audio | 8.99M | 7M | 1.99M | 22.1% | 1 |

**Top Revenue Products (Ranked):**
1. iPhone 15 Pro - 89.97M (16.6% margin)
2. Apple Watch S9 - 45.96M (21.7% margin)
3. Dell XPS 13 - 38.99M (17.9% margin)
4. Samsung S23 Ultra - 31.99M (15.6% margin)
5. MacBook Air M3 - 30.99M (16.1% margin)

**Use Cases:**
- Reports page with date range filters
- Profit margin analysis
- Category performance comparison
- Excel export functionality
- Business insights

---

### 8. **settings.json** - App & System Settings

**Mục đích:** Cấu hình ứng dụng và hệ thống

**Nội dung:**
- ✅ **App Settings**: User preferences
- ✅ **System Settings**: Application config
- ✅ **Business Settings**: Store information

**App Settings:**
```json
{
  "userId": "admin-user-id",
  "pageSize": 10,
  "lastOpenedPage": "DASHBOARD",
  "theme": "LIGHT",
  "language": "vi",
  "notifications": {
    "emailNotifications": true,
    "lowStockAlerts": true,
    "newOrderAlerts": true,
    "lowStockThreshold": 10
  }
}
```

**System Settings:**
- Application Name: "MyShop 2025"
- Version: "1.0.0"
- Default Currency: "VND"
- Tax Rate: 10%
- Trial Period: 15 days
- Feature flags (Google Login, Email Verification, etc.)

**Business Settings:**
- Store Name: "MyShop Electronics"
- Address: "123 Nguyễn Huệ, Quận 1, TP.HCM"
- Contact: 028-3822-1234, contact@myshop.com
- Bank Info: Vietcombank - 0123456789

**Use Cases:**
- Settings page display
- Theme switcher
- Language selection
- Notification preferences
- Invoice printing (business info)

---

### 9. **customers.json** - Customer Management

**Mục đích:** Quản lý thông tin khách hàng

**Nội dung:**
- ✅ **8 customers** từ orders

**Customer Types:**
- **VIP** (3 customers): Lê Thị Mai, Vũ Minh Khang, Bùi Văn Hùng
- **REGULAR** (5 customers): Còn lại

**Customer Highlights:**

| Name | Phone | Total Orders | Total Spent | Last Order | Type |
|------|-------|--------------|-------------|------------|------|
| Bùi Văn Hùng | 0989012345 | 1 | **80M** | 10/30 | VIP 👑 |
| Lê Thị Mai | 0923456789 | 1 | 37.98M | 11/07 | VIP 👑 |
| Trương Thị Hồng | 0978901234 | 1 | 38.99M | 11/02 | REGULAR |
| Nguyễn Văn An | 0912345678 | 1 | 29.99M | 11/05 | REGULAR |
| Vũ Minh Khang | 0967890123 | 1 | 21M | 11/06 | VIP 👑 |

**Pending Customers (totalSpent = 0):**
- Phạm Minh Tuấn (order CREATED)
- Hoàng Văn Dũng (order CREATED)
- Đỗ Thị Lan (order CANCELLED)

**Fields:**
- id, name, phone, email, address
- totalOrders, totalSpent, lastOrderDate
- customerType (VIP/REGULAR), createdAt, updatedAt

**Use Cases:**
- Customer list page
- VIP customer management
- Customer search by phone/email
- Purchase history

---

## 🎯 Use Cases & Workflows

### Authentication Flow
1. **Login** → auth.json users
2. **Trial Activation** → customer1 (active trial)
3. **Admin Code Verification** → admin codes list
4. **Email Verification** → newuser (unverified)

### Product Management
1. **Product List** → products.json (10 items)
2. **Low Stock Alert** → Logitech MX Master (3 units)
3. **Out of Stock** → Nintendo Switch (0 units)
4. **Category Filter** → categories.json (8 categories)

### Order Management
1. **Create Order** → CREATED status orders
2. **Process Payment** → Mark as PAID
3. **Cancel Order** → CANCELLED with reason
4. **Sales Agent View** → Filter by salesAgentId

### Dashboard
1. **Today Stats** → todayOrders, todayRevenue
2. **Low Stock Widget** → Top 3 low stock products
3. **Top Selling** → Top 5 products by revenue
4. **Revenue Chart** → Daily/Weekly/Monthly data

### Reports
1. **Sales Report** → Filter by date, product, category
2. **Revenue Report** → Summary + breakdown
3. **Profit Analysis** → Cost vs Revenue vs Profit
4. **Top Products** → Ranked by revenue

---

## 📊 Data Statistics

| Entity | Count | Status |
|--------|-------|--------|
| Users | 5 | ✅ Complete |
| Profiles | 5 | ✅ Complete |
| Admin Codes | 3 | ✅ Complete |
| Categories | 8 | ✅ Complete |
| Products | 10 | ✅ Complete (2 low/out stock) |
| Orders | 8 | ✅ Complete (5 paid, 2 pending, 1 cancelled) |
| Order Items | 12 | ✅ Complete |
| Customers | 8 | ✅ Complete |

**Total Revenue:** 329.95M VND  
**Total Profit:** 60.15M VND  
**Profit Margin:** 18.2%

---

## 🔄 Relationship Mapping

```
Users (auth.json)
  ↓ userId
Profiles (profiles.json)

Users → Orders (salesAgentId)
Orders → Products (order items)
Products → Categories

Orders → Customers (customerName/Phone)
Dashboard ← Orders + Products
Reports ← Orders + Products
```

---

## 🚀 Next Steps

### Phase 1: Tạo Mock Repository Classes
```csharp
// src/MyShop.Plugins/Mocks/Repositories/
MockProductRepository.cs   ← Load products.json
MockOrderRepository.cs     ← Load orders.json
MockCategoryRepository.cs  ← Load categories.json
MockDashboardRepository.cs ← Load dashboard.json
MockReportRepository.cs    ← Load reports.json
```

### Phase 2: Dependency Injection Setup
```csharp
// Bootstrapper.cs or App.xaml.cs
services.AddSingleton<IProductRepository, MockProductRepository>();
services.AddSingleton<IOrderRepository, MockOrderRepository>();
// ... other repositories
```

### Phase 3: ViewModel Integration
```csharp
// ProductsViewModel.cs
var result = await _productRepository.GetAllAsync();
// Will load from products.json via MockProductRepository
```

---

## 📝 Validation & Testing

### ✅ Data Integrity Checks
- [x] All GUIDs are unique
- [x] Foreign keys match (userId, productId, categoryId)
- [x] Dates are in chronological order
- [x] Phone numbers are Vietnamese format (09xx, 10 digits)
- [x] Prices are realistic (importPrice < sellingPrice)
- [x] Stock quantities match order items
- [x] Order totals calculated correctly

### ✅ Business Logic Validation
- [x] Profit margin = (sellingPrice - importPrice) / sellingPrice
- [x] Commission <= profit margin
- [x] Trial period = 15 days
- [x] Low stock threshold = 10 units
- [x] Order status flow: CREATED → PAID/CANCELLED

---

## 🎨 Image Assets Placeholder

All `imageUrl` fields use placeholder paths:
```
/assets/images/products/{product-slug}.jpg
```

**Recommended replacements:**
- Use UI Avatars for user avatars (already implemented in profiles.json)
- Use placeholder image services (picsum.photos, placeholder.com)
- Or replace with actual product images later

---

## 📚 References

- **API Contract**: `docs-temp/structure/api/00-FRONTEND-API-REQUIREMENTS.md`
- **Mock Data Guide**: `docs-temp/mock-docs/MOCK-DATA-GUIDE.md`
- **DTOs**: `src/MyShop.Shared/DTOs/Responses/`
- **Existing Mock**: `src/MyShop.Plugins/Mocks/Data/MockAuthData.cs`

---

## 📅 Metadata

**Created:** November 10, 2025  
**Author:** AI Assistant (GitHub Copilot)  
**Version:** 1.0.0  
**Project:** MyShop 2025 - WinUI Conversion  
**Total Files:** 9 JSON files  
**Total Data Points:** 100+ records  

---

**🎯 Mục tiêu đã đạt được:**
✅ Tạo 9 file JSON mock data hoàn chỉnh  
✅ 5-10 mẫu dữ liệu thực tế cho mỗi loại  
✅ Bám sát API contract từ 00-FRONTEND-API-REQUIREMENTS.md  
✅ Format hợp lệ với field names trong DTOs  
✅ Dữ liệu nhất quán và có logic nghiệp vụ đúng  
✅ Tài liệu hướng dẫn sử dụng chi tiết  

**Sẵn sàng cho Phase tiếp theo:** Tạo Mock Repository classes để load JSON data! 🚀
