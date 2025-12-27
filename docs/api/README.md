# Reports API Module - Documentation Index

**Version:** 1.0  
**Last Updated:** December 24, 2025  
**Status:** ? Production Ready

---

## Overview

The Reports API Module provides comprehensive reporting capabilities for both Admin and Sales Agent roles, enabling data-driven decision making and performance tracking.

### Key Features

- **?? Admin Reports** - Platform-wide analytics and metrics
- **?? Sales Agent Reports** - Personal performance tracking
- **?? Revenue Trends** - Time-series revenue analysis
- **?? Category Analytics** - Product category performance
- **?? Top Products** - Best-selling product identification
- **?? Commission Tracking** - Automated commission calculations

---

## Available Endpoints

### Admin Reports

**Endpoint:** `GET /api/v1/dashboard/admin-reports`  
**Role:** Admin  
**Purpose:** Platform-wide reporting and analytics

**Features:**
- Custom date range (`from` and `to` parameters)
- Revenue trends (daily breakdown)
- Orders by category analysis
- Product ratings distribution (placeholder)
- Salesperson contributions (top 10)
- Product summary (paginated)
- Category filtering for products

**Documentation:**
- [Implementation Guide](./admin-reports-implementation.md)
- [Testing Guide](./admin-reports-testing.md)
- [Summary](./admin-reports-summary.md)

---

### Sales Agent Reports

**Endpoint:** `GET /api/v1/dashboard/sales-agent-reports`  
**Role:** SalesAgent  
**Purpose:** Personal performance tracking for sales agents

**Features:**
- Preset time periods (day/week/month/year)
- Revenue trends (variable granularity)
- Orders by category with commission
- Top 5 products
- Category filtering
- Automatic user isolation

**Documentation:**
- [Implementation Guide](./sales-agent-reports-implementation.md)
- [Testing Guide](./sales-agent-reports-testing.md)
- [Summary](./sales-agent-reports-summary.md)

---

## Quick Start

### Prerequisites

1. Running MyShop API server
2. Valid JWT token with appropriate role
3. API client (Postman, cURL, or HTTP client)

### Admin Reports Example

```bash
# Get reports for last 7 days
curl -X GET "https://localhost:7120/api/v1/dashboard/admin-reports?from=2025-12-17T00:00:00Z&to=2025-12-24T23:59:59Z" \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

### Sales Agent Reports Example

```bash
# Get weekly report
curl -X GET "https://localhost:7120/api/v1/dashboard/sales-agent-reports?period=week" \
  -H "Authorization: Bearer SALESAGENT_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

---

## Architecture

### Data Flow

```
???????????????       ????????????????       ???????????????
?   Client    ?????????  Controller  ?????????   Service   ?
?  (Frontend) ?       ? (Validation) ?       ?   (Logic)   ?
???????????????       ????????????????       ???????????????
                                                     ?
                                                     ?
                                              ???????????????
                                              ?  Database   ?
                                              ?  (Orders)   ?
                                              ???????????????
```

### Components

1. **Controllers** (`src/MyShop.Server/Controllers/DashboardController.cs`)
   - Request validation
   - Authorization checks
   - Error handling

2. **Services** (`src/MyShop.Server/Services/Implementations/DashboardService.cs`)
   - Business logic
   - Data aggregation
   - Commission calculations

3. **DTOs** (`src/MyShop.Shared/DTOs/Responses/`)
   - Request/Response models
   - Data contracts

4. **Database** (`ShopContext`)
   - Order data
   - Product data
   - User data

---

## Comparison Matrix

| Feature | Admin Reports | Sales Agent Reports |
|---------|--------------|---------------------|
| **Authorization** | Admin | SalesAgent |
| **Data Scope** | Platform-wide | Personal only |
| **Time Filter** | Custom range (`from`/`to`) | Preset periods (day/week/month/year) |
| **Granularity** | Daily | Variable (hourly to monthly) |
| **Sections** | 5 (trend, category, ratings, agents, products) | 3 (trend, category, products) |
| **Pagination** | Yes (product summary) | No (top 5) |
| **Commission View** | Platform (5%) | Agent (95%) |
| **Category Filter** | ? Yes | ? Yes |
| **Product Ratings** | ? Yes (placeholder) | ? No |
| **Salesperson Data** | ? Top 10 | ? N/A |
| **Use Case** | Strategic planning | Performance tracking |

---

## Time Period Formats

### Admin Reports
- **Custom Date Range** - Any `from` to `to` dates
- **Granularity** - Daily breakdown
- **Example** - Dec 1-31, 2025

### Sales Agent Reports

| Period | Range | Granularity | Data Points |
|--------|-------|-------------|-------------|
| day | Today | 3-hour blocks | 8 |
| week | Mon-Sun | Daily | 7 |
| month | 1st-now | Weekly | 4-5 |
| year | Jan 1-now | Monthly | 1-12 |

---

## Response Format

Both endpoints use the standard API response format:

```json
{
  "success": true | false,
  "message": "Description of result",
  "code": 200 | 400 | 401 | 403 | 500,
  "result": {
    // Report data here
  }
}
```

---

## Error Handling

### Common Error Codes

| Code | Meaning | Admin Reports | Sales Agent Reports |
|------|---------|---------------|-------------------|
| 200 | Success | ? Data returned | ? Data returned |
| 400 | Bad Request | Invalid date range, pagination | Invalid period |
| 401 | Unauthorized | No token or expired | No token or expired |
| 403 | Forbidden | Not an admin | Not a sales agent |
| 500 | Server Error | Internal error | Internal error |

### Error Response Example

```json
{
  "success": false,
  "message": "Invalid date range. Start date must be before end date.",
  "code": 400,
  "result": null
}
```

---

## Security

### Authentication
- **Required:** JWT Bearer token
- **Header:** `Authorization: Bearer {token}`

### Authorization
- **Admin Reports:** Requires `Admin` role
- **Sales Agent Reports:** Requires `SalesAgent` role

### Data Isolation
- **Admin:** Can view all platform data
- **Sales Agent:** Automatically filtered to own data only

---

## Performance

### Expected Response Times

| Endpoint | Typical | 95th Percentile |
|----------|---------|-----------------|
| Admin Reports | 1-2s | < 3s |
| Sales Agent Reports | 500ms-1s | < 2s |

### Optimization Strategies

1. **Database Indexes**
   - Composite indexes on (sale_agent_id, order_date)
   - Indexes on foreign keys

2. **Caching**
   - 5-minute TTL for report data
   - Invalidate on order updates

3. **Query Optimization**
   - Early filtering by user/date
   - In-memory aggregation for small datasets

---

## Testing

### Postman Collection

Import the Postman collection from `test/postman/reports-api.json` (to be created)

### Test Data

Seed the database with test orders:
```bash
cd src/MyShop.Data
dotnet ef database update
# Run seed script if available
```

### Automated Testing

Run integration tests:
```bash
cd test/MyShop.Integration.Tests
dotnet test --filter "Category=Reports"
```

---

## Known Limitations

1. **Product Ratings** - Currently returns placeholder data (0)
   - Reason: Rating system not implemented in database
   - Impact: Both endpoints affected
   - Timeline: Future enhancement

2. **Time Zones** - All dates/times in UTC
   - Impact: Frontend must convert to local time
   - Workaround: Client-side conversion

3. **Historical Data** - Limited by order data availability
   - Impact: New installations have no data
   - Workaround: Seed test data

---

## Future Roadmap

### Phase 2 (Q1 2026)
- [ ] Product rating system implementation
- [ ] Real rating data in reports
- [ ] Enhanced filtering options

### Phase 3 (Q2 2026)
- [ ] Export to PDF/Excel
- [ ] Scheduled email reports
- [ ] Custom dashboard widgets

### Phase 4 (Q3 2026)
- [ ] Real-time updates (WebSockets)
- [ ] Predictive analytics
- [ ] AI-powered insights

---

## Documentation Files

### Admin Reports
- [admin-reports-implementation.md](./admin-reports-implementation.md) - Complete API spec
- [admin-reports-testing.md](./admin-reports-testing.md) - Testing guide
- [admin-reports-summary.md](./admin-reports-summary.md) - Implementation summary

### Sales Agent Reports
- [sales-agent-reports-implementation.md](./sales-agent-reports-implementation.md) - Complete API spec
- [sales-agent-reports-testing.md](./sales-agent-reports-testing.md) - Testing guide
- [sales-agent-reports-summary.md](./sales-agent-reports-summary.md) - Implementation summary

### Product Rating (Future)
- [product-rating-implementation.md](./product-rating-implementation.md) - Migration guide

---

## Support

### Questions & Issues
- **GitHub Issues:** https://github.com/05-victor/MyShop2025/issues
- **Email:** support@myshop2025.com

### Contributing
- Fork the repository
- Create feature branch
- Submit pull request

### Code Owners
- Backend Team

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-24 | Initial release with Admin and Sales Agent reports |

---

**Last Updated:** December 24, 2025  
**Maintained By:** MyShop Backend Team  
**License:** Proprietary
