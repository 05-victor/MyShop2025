# Walmart Sales Forecasting - Database Schema Documentation

This document describes the database schema required for the Walmart sales forecasting system that uses a Deep Neural Network (DNN) model to predict weekly sales.

---

## Overview

The forecasting system requires several database tables to:
1. Capture web sales transactions in real-time
2. Store aggregated weekly sales data for model training
3. Maintain reference data (stores, departments)
4. Track external features (temperature, fuel prices, economic indicators)
5. Enable inference API to make predictions

---

## Database Schema

### 1. Sales Transactions Table (Primary)

This table stores individual web transactions. Data is aggregated daily/weekly for model input.

```sql
CREATE TABLE sales_transactions (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    store_id INT NOT NULL COMMENT 'Store ID (1-45)',
    dept_id INT NOT NULL COMMENT 'Department ID (1-99)',
    transaction_date DATE NOT NULL COMMENT 'Transaction date (YYYY-MM-DD)',
    is_holiday BOOLEAN NOT NULL COMMENT 'Is this a holiday week?',
    weekly_sales DECIMAL(10,2) NOT NULL COMMENT 'Weekly sales amount',
    
    -- Transaction details (for reporting/analysis)
    customer_id VARCHAR(50) COMMENT 'Customer identifier',
    transaction_id VARCHAR(100) UNIQUE COMMENT 'Unique transaction ID',
    item_count INT COMMENT 'Number of items in transaction',
    total_amount DECIMAL(10,2) COMMENT 'Total transaction amount',
    payment_method VARCHAR(50) COMMENT 'Payment method used',
    
    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes for query performance
    INDEX idx_store_dept_date (store_id, dept_id, transaction_date),
    INDEX idx_transaction_date (transaction_date),
    INDEX idx_store (store_id),
    INDEX idx_dept (dept_id)
);
```

**Field Descriptions:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `store_id` | INT | Yes | Store identifier (1-45) |
| `dept_id` | INT | Yes | Department identifier (1-99) |
| `transaction_date` | DATE | Yes | Date of transaction |
| `is_holiday` | BOOLEAN | Yes | Whether this week is a holiday week |
| `weekly_sales` | DECIMAL | Yes | Sales amount for the week |
| `customer_id` | VARCHAR | No | Customer identifier for analytics |
| `transaction_id` | VARCHAR | Yes (unique) | Unique transaction reference |
| `item_count` | INT | No | Number of items purchased |
| `total_amount` | DECIMAL | No | Total transaction value |
| `payment_method` | VARCHAR | No | Payment method used |

---

### 2. Stores Reference Table

Reference data for store characteristics. Loaded once per store.

```sql
CREATE TABLE stores (
    store_id INT PRIMARY KEY COMMENT 'Store ID (1-45)',
    store_type CHAR(1) NOT NULL COMMENT 'Store type: A, B, or C',
    size INT NOT NULL COMMENT 'Store size in square feet',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_store_type (store_type)
);
```

**Sample Data:**
```csv
store_id,store_type,size
1,A,151315
2,A,202307
3,B,37392
```

**Store Types:**
- Type A: Large stores (>150k sq ft)
- Type B: Medium stores (50k-150k sq ft)  
- Type C: Small stores (<50k sq ft)

---

### 3. External Features Table (Admin-managed)

This table stores external features that impact sales predictions. Updated periodically by administrators.

```sql
CREATE TABLE external_features (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    store_id INT NOT NULL COMMENT 'Store ID',
    feature_date DATE NOT NULL COMMENT 'Date for these features',
    
    -- Weather & economic indicators
    temperature DECIMAL(5,2) COMMENT 'Temperature in Fahrenheit',
    fuel_price DECIMAL(5,3) COMMENT 'Fuel price per gallon',
    cpi DECIMAL(10,7) COMMENT 'Consumer Price Index',
    unemployment DECIMAL(5,3) COMMENT 'Unemployment rate (%)',
    
    -- Markdown promotions
    markdown1 DECIMAL(10,2) DEFAULT 0 COMMENT 'Markdown amount 1',
    markdown2 DECIMAL(10,2) DEFAULT 0 COMMENT 'Markdown amount 2',
    markdown3 DECIMAL(10,2) DEFAULT 0 COMMENT 'Markdown amount 3',
    markdown4 DECIMAL(10,2) DEFAULT 0 COMMENT 'Markdown amount 4',
    markdown5 DECIMAL(10,2) DEFAULT 0 COMMENT 'Markdown amount 5',
    
    -- Holiday indicator
    is_holiday BOOLEAN DEFAULT FALSE COMMENT 'Is this a holiday week?',
    
    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Unique constraint to prevent duplicates
    UNIQUE KEY unique_store_date (store_id, feature_date),
    INDEX idx_feature_date (feature_date),
    INDEX idx_store_feature (store_id, feature_date)
);
```

**Field Descriptions:**

| Field | Type | Description |
|-------|------|-------------|
| `store_id` | INT | Store identifier |
| `feature_date` | DATE | Date for these external features |
| `temperature` | DECIMAL | Weather data (°F) |
| `fuel_price` | DECIMAL | Fuel price ($/gallon) |
| `cpi` | DECIMAL | Consumer Price Index |
| `unemployment` | DECIMAL | Unemployment rate (%) |
| `markdown1-5` | DECIMAL | Promotion amounts |
| `is_holiday` | BOOLEAN | Holiday week flag |

---

### 4. Aggregate Statistics Table (Generated)

This table is automatically generated from historical sales data. Contains calculated statistics per Store+Department combination.

```sql
CREATE TABLE store_dept_stats (
    store_id INT NOT NULL,
    dept_id INT NOT NULL,
    max_sales DECIMAL(10,2) COMMENT 'Maximum weekly sales',
    min_sales DECIMAL(10,2) COMMENT 'Minimum weekly sales',
    mean_sales DECIMAL(10,2) COMMENT 'Mean weekly sales',
    median_sales DECIMAL(10,2) COMMENT 'Median weekly sales',
    std_sales DECIMAL(10,2) COMMENT 'Standard deviation of sales',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    PRIMARY KEY (store_id, dept_id),
    INDEX idx_store (store_id),
    INDEX idx_dept (dept_id)
);
```

**Generated from historical training data** (not web transactions). Used for model inference when predicting sales for new dates.

---

## Data Collection Guidelines

### Batch Updates

Sales transactions are aggregated in **batch mode** at end of day/week:

**Daily Aggregation (recommended):**
```sql
-- Aggregate daily transactions into weekly sales
INSERT INTO weekly_sales_summary (store_id, dept_id, week_start_date, weekly_sales, is_holiday)
SELECT 
    store_id,
    dept_id,
    DATE_SUB(DATE(transaction_date), INTERVAL DAYOFWEEK(transaction_date)-1 DAY) as week_start_date,
    SUM(total_amount) as weekly_sales,
    MAX(is_holiday) as is_holiday
FROM sales_transactions
WHERE transaction_date >= CURDATE() - INTERVAL 7 DAY
GROUP BY store_id, dept_id, week_start_date;
```

**Holiday Week Marking:**
```sql
-- Mark holiday weeks based on calendar
UPDATE weekly_sales_summary 
SET is_holiday = TRUE 
WHERE week_start_date IN (
    '2012-02-12', -- Super Bowl
    '2012-09-07', -- Labor Day
    '2012-11-23', -- Thanksgiving
    '2012-12-28'  -- Christmas
);
```

### Weekly Aggregation

For model training, aggregate transactions by week:

```sql
SELECT 
    store_id,
    dept_id,
    WEEK(transaction_date, 1) as week_number,
    YEAR(transaction_date) as year,
    SUM(total_amount) as weekly_sales,
    MAX(is_holiday) as is_holiday
FROM sales_transactions
WHERE transaction_date BETWEEN '2012-01-01' AND '2012-12-31'
GROUP BY store_id, dept_id, week_number, year;
```

---

## Integration with ML Model

### Model Input Requirements

The DNN model expects 4 input parameters:
1. `store_id` (INT) - 1 to 45
2. `dept_id` (INT) - 1 to 99
3. `date` (DATE) - Week start date (YYYY-MM-DD)
4. `is_holiday` (BOOLEAN) - Holiday week flag

### Model Output

The model returns:
```json
{
    "weekly_sales": 9876.54,
    "store": 1,
    "dept": 1,
    "date": "2012-11-02",
    "message": "success"
}
```

### Data Flow

```
Web Sales → sales_transactions table
    ↓
Aggregate to weekly_sales_summary
    ↓
Export to CSV (train.csv format)
    ↓
Train/retrain DNN model
    ↓
Deploy model for inference
    ↓
API receives requests → Returns predictions
```

---

## Sample Queries

### 1. Get Weekly Sales Summary

```sql
SELECT 
    s.store_id,
    s.dept_id,
    DATE(s.transaction_date) as week_start,
    SUM(s.total_amount) as weekly_sales,
    MAX(s.is_holiday) as is_holiday,
    COUNT(*) as transaction_count
FROM sales_transactions s
WHERE s.transaction_date >= '2012-11-01' 
  AND s.transaction_date < '2012-11-08'
GROUP BY s.store_id, s.dept_id, DATE(s.transaction_date);
```

### 2. Export Data for Model Training

```sql
SELECT 
    store_id,
    dept_id,
    DATE_FORMAT(week_start_date, '%Y-%m-%d') as date,
    is_holiday,
    weekly_sales
FROM weekly_sales_summary
WHERE week_start_date >= '2010-02-05'
  AND week_start_date <= '2012-10-26'
ORDER BY store_id, dept_id, week_start_date;
```

### 3. Check Data Completeness

```sql
-- Count stores with missing data for a given week
SELECT 
    s.store_id,
    COUNT(DISTINCT s.dept_id) as dept_count,
    COUNT(CASE WHEN wss.weekly_sales IS NULL THEN 1 END) as missing_sales
FROM stores s
LEFT JOIN weekly_sales_summary wss ON s.store_id = wss.store_id
WHERE wss.week_start_date = '2012-11-02'
GROUP BY s.store_id;
```

### 4. Update External Features (Admin)

```sql
-- Update external features for a specific date
INSERT INTO external_features (
    store_id, feature_date, temperature, fuel_price, cpi, unemployment, is_holiday
)
VALUES (
    1, '2025-10-28', 45.5, 2.55, 215.0, 8.0, FALSE
)
ON DUPLICATE KEY UPDATE
    temperature = VALUES(temperature),
    fuel_price = VALUES(fuel_price),
    cpi = VALUES(cpi),
    unemployment = VALUES(unemployment),
    is_holiday = VALUES(is_holiday),
    updated_at = CURRENT_TIMESTAMP;
```

### 5. Recent Sales Dashboard

```sql
SELECT 
    s.store_id,
    s.dept_id,
    SUM(s.total_amount) as total_sales,
    COUNT(*) as transaction_count,
    AVG(s.item_count) as avg_items_per_transaction,
    MAX(s.transaction_date) as last_sale_date
FROM sales_transactions s
WHERE s.created_at >= DATE_SUB(NOW(), INTERVAL 7 DAY)
GROUP BY s.store_id, s.dept_id
ORDER BY total_sales DESC;
```

---

## API Integration

### Inference Endpoint

```
POST /api/predict
Content-Type: application/json

{
    "store": 1,
    "dept": 1,
    "date": "2012-11-02",
    "is_holiday": false
}
```

The API:
1. Loads model from `models/dnn_regressor.json` and weights from `models/dnn_regressor.h5`
2. Fetches external features from `external_features` table
3. Gets aggregate stats from `store_dept_stats` table
4. Runs inference and returns predicted weekly sales

---

## Maintenance & Updates

### External Features Update Schedule

- **Temperature**: Daily updates from weather API
- **Fuel_Price**: Weekly updates from energy API
- **CPI**: Monthly updates from government statistics
- **Unemployment**: Monthly updates from labor statistics
- **MarkDown1-5**: Updated manually when promotions run

### Model Retraining

Trigger retraining when:
- New weekly sales data exceeds threshold (e.g., 10k new records)
- Performance degrades (validation error increases)
- Quarterly scheduled retraining

**Data Export for Retraining:**
```bash
mysql -u user -p database -e "
SELECT store_id, dept_id, date, is_holiday, weekly_sales 
FROM weekly_sales_summary 
ORDER BY date
" > data/train_export.csv
```

---

## Notes

- **Store IDs**: Currently using IDs 1-45 from training data
- **Department IDs**: Range 1-99 (81 unique departments in training data)
- **Week Definition**: Weeks start on Sunday (ISO week)
- **Holiday Dates**: Fixed annual holidays (Super Bowl, Labor Day, Thanksgiving, Christmas)
- **Historical Data**: Training period 2010-02-05 to 2012-10-26 (143 weeks)

---

## Contact

For questions about the database schema or model requirements, please refer to the model documentation in `notebook/Walmart_Time_Series_Forecast.ipynb`.

