# Forecasting APIs

2 REST APIs để dự đoán: **Demand Forecasting** (units_sold) và **Price Forecasting** (Weekly_Sales).

## Cài đặt & Chạy



```bash
pip install -r api/requirements.txt
Run all file MyShop2025/WPExtension/Demand Forecasting/Demand forecasting.ipynb
uvicorn api.main:app --host 0.0.0.0 --port 8000
```

API chạy tại: `http://localhost:8000`

## Sử dụng

### Demand Forecasting

```bash
POST /api/demand-forecast/predict
```

**Request**:
```json
{
  "week": "17/01/11",
  "store_id": 8091,
  "sku_id": 216418,
  "base_price": 111.8625,
  "total_price": 99.0375,
  "is_featured_sku": 0,
  "is_display_sku": 0
}
```

**Response**:
```json
{
  "predicted_units_sold": 21.16,
  "strategy_used": "lightgbm",
  "status": "success"
}
```

**Lưu ý**: `total_price` optional → tự động dùng `base_price` nếu thiếu.

### Price Forecasting

```bash
POST /api/price-forecast/predict
```

**Request**:
```json
{
  "Store": 1,
  "Dept": 1,
  "Date": "2012-11-02",
  "strategy": "linear"
}
```

**Response**:
```json
{
  "predicted_weekly_sales": 9876.54,
  "store": 1,
  "dept": 1,
  "date": "2012-11-02",
  "strategy_used": "linear",
  "status": "success"
}
```

**Strategies**: `"linear"` (default) hoặc `"dnn"`

**Lưu ý**: Nếu thiếu features (Temperature, Fuel_Price...) → tự động tìm ngày gần nhất trong dataset.

## Test

```bash
python test_api.py
```

## API Docs

- Swagger: `http://localhost:8000/docs`
- Health: `http://localhost:8000/health`

## Examples

### Python

```python
import requests

# Demand
requests.post("http://localhost:8000/api/demand-forecast/predict", json={
    "week": "17/01/11", "store_id": 8091, "sku_id": 216418,
    "base_price": 111.8625, "total_price": 99.0375
}).json()

# Price
requests.post("http://localhost:8000/api/price-forecast/predict", json={
    "Store": 1, "Dept": 1, "Date": "2012-11-02"
}).json()
```

### C#

```csharp
var client = new HttpClient();
var json = JsonSerializer.Serialize(new { Store = 1, Dept = 1, Date = "2012-11-02" });
var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = await client.PostAsync("http://localhost:8000/api/price-forecast/predict", content);
var result = await response.Content.ReadAsStringAsync();
```
