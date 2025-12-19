# 📘 AV Demand Forecasting - Inference Guide

**Phiên bản:** 2.0 - Simplified  
**Cập nhật:** November 2024  
**Dành cho:** Người dùng không cần background kỹ thuật

---

## 🎯 Model này làm gì?

Dự đoán **số lượng sản phẩm sẽ bán được** (units_sold) cho mỗi:
- Cửa hàng (store)
- Sản phẩm (SKU)
- Tuần cụ thể (week)

**Ví dụ:** Cửa hàng số 8091 sẽ bán được bao nhiêu sản phẩm số 216418 vào tuần 16/07/13?  
→ Model trả lời: **21 sản phẩm**

---

## 📥 INPUT - Bạn cần cung cấp gì?

### 1. File CSV với 8 thông tin cơ bản

| Tên cột | Ý nghĩa | Ví dụ | Bắt buộc? |
|---------|---------|-------|-----------|
| `record_ID` | Mã định danh duy nhất | 212645 | ✅ Có |
| `week` | Tuần bắt đầu (ngày/tháng/năm) | 16/07/13 | ✅ Có |
| `store_id` | Mã cửa hàng | 8091 | ✅ Có |
| `sku_id` | Mã sản phẩm | 216418 | ✅ Có |
| `total_price` | Giá bán thực tế | 108.30 | ⚠️ Có thể thiếu |
| `base_price` | Giá gốc | 108.30 | ✅ Có |
| `is_featured_sku` | Sản phẩm được quảng cáo? (0=Không, 1=Có) | 0 | ✅ Có |
| `is_display_sku` | Sản phẩm được trưng bày đặc biệt? (0=Không, 1=Có) | 0 | ✅ Có |

### 2. Format tuần (week)

**Quan trọng:** Phải đúng format `DD/MM/YY`

✅ Đúng:
- `16/07/13` (ngày 16, tháng 7, năm 2013)
- `23/01/12` (ngày 23, tháng 1, năm 2012)

❌ Sai:
- `2013-07-16` (sai format)
- `07/16/13` (tháng trước ngày - format Mỹ)
- `16-07-2013` (dùng dấu gạch ngang)

### 3. Ví dụ file CSV đầu vào

```
record_ID,week,store_id,sku_id,total_price,base_price,is_featured_sku,is_display_sku
212645,16/07/13,8091,216418,108.30,108.30,0,0
212646,16/07/13,8091,216419,109.01,109.01,0,0
212647,16/07/13,8091,216425,120.50,133.95,1,1
212648,16/07/13,8091,216233,,133.95,0,0
```

**Lưu ý dòng cuối:** `total_price` bị thiếu (để trống) → Không sao, hệ thống tự động dùng `base_price` thay thế.

---

## 📤 OUTPUT - Bạn nhận được gì?

### 1. File CSV với 2 cột

| Tên cột | Ý nghĩa | Ví dụ |
|---------|---------|-------|
| `record_ID` | Mã định danh (giống input) | 212645 |
| `units_sold` | **Số lượng dự đoán sẽ bán** | 21.16 |

### 2. Ví dụ file kết quả

```
record_ID,units_sold
212645,21.163058
212646,23.675132
212647,34.839319
212648,31.773779
```

### 3. Giải thích kết quả

- **Record 212645:** Dự đoán bán được **21.16 sản phẩm**
- **Record 212647:** Dự đoán bán được **34.84 sản phẩm** (cao hơn vì có quảng cáo + trưng bày)

**Tại sao là số thập phân?** Đây là giá trị trung bình dự đoán từ 10 models khác nhau. Bạn có thể làm tròn nếu cần số nguyên.

---

## 🔧 Files cần có để chạy

Ngoài file CSV input, bạn cần có sẵn:

### 1. Models (10 files)
```
weight/
├── model_fold_0.txt
├── model_fold_1.txt
├── model_fold_2.txt
├── model_fold_3.txt
├── model_fold_4.txt
├── model_fold_5.txt
├── model_fold_6.txt
├── model_fold_7.txt
├── model_fold_8.txt
└── model_fold_9.txt
```

**Giải thích:** 10 models khác nhau, mỗi model học từ một phần dữ liệu. Kết quả cuối = trung bình 10 dự đoán → Chính xác hơn.

### 2. Encoders (3 files)
```
encoders/
├── store_encoder.pkl
├── sku_encoder.pkl
└── time_encoder.pkl
```

**Giải thích:** Các file chuyển đổi dữ liệu (store_id, sku_id, thời gian) sang dạng model hiểu được.

### 3. Config (optional)
```
weight/config.json
```

**Giải thích:** Cấu hình model (không bắt buộc).

---

## 📊 Model hoạt động như thế nào?

### Quá trình đơn giản:

```
Bạn cung cấp:
  ├─ Cửa hàng nào? (store_id)
  ├─ Sản phẩm nào? (sku_id)
  ├─ Tuần nào? (week)
  ├─ Giá bao nhiêu? (price)
  └─ Có quảng cáo/trưng bày không? (featured/display)
       ↓
Model phân tích:
  ├─ Lịch sử bán hàng của cửa hàng này
  ├─ Lịch sử bán của sản phẩm này
  ├─ Mùa vụ (tháng nào, tuần nào trong năm)
  ├─ Mức giảm giá (base_price - total_price)
  └─ Hiệu ứng quảng cáo/trưng bày
       ↓
Model dự đoán:
  └─ Số lượng sẽ bán được: XX.XX sản phẩm
```

### Model học từ đâu?

- **76 cửa hàng** khác nhau
- **28 loại sản phẩm** khác nhau
- **130 tuần** lịch sử (2011-2013)
- **Hơn 100,000 giao dịch** đã xảy ra

---

## 📈 Độ chính xác

### Metric sử dụng: RMSLE (Root Mean Squared Logarithmic Error)

**Giải thích đơn giản:**
- Số càng nhỏ = Model càng chính xác
- So sánh sai số theo **tỷ lệ %** thay vì số tuyệt đối
- Ví dụ: Sai 5 sản phẩm khi dự đoán 50 sản phẩm = Nghiêm trọng hơn sai 5 sản phẩm khi dự đoán 500 sản phẩm

### Kết quả hiện tại:

| Đánh giá | Giá trị RMSLE | Ý nghĩa |
|----------|---------------|---------|
| **Validation** | ~327 | Độ chính xác khi test trên training data |
| **Test** | ~360 | Độ chính xác trên data chưa từng thấy |

**Có tốt không?** Khá tốt cho bài toán này. Có thể cải thiện thêm bằng:
- Thêm features (ví dụ: thông tin khách hàng, thời tiết, ngày lễ)
- Tune hyperparameters
- Thử models khác

---

## ⏱️ Tốc độ

| Số lượng dữ liệu | Thời gian xử lý |
|------------------|-----------------|
| 1 sample | < 1 giây |
| 1,000 samples | ~1 giây |
| 13,860 samples (test set) | ~5 giây |

**Yêu cầu phần cứng:** CPU bình thường, 8GB RAM. Không cần GPU.

---

## ❓ Câu hỏi thường gặp

### 1. Tại sao cần 10 models thay vì 1?

**Trả lời:** Giống như hỏi ý kiến 10 chuyên gia thay vì 1 người → Kết quả trung bình đáng tin hơn, giảm rủi ro dự đoán sai.

### 2. Model có dự đoán cho cửa hàng/sản phẩm mới không?

**Trả lời:** Có thể, nhưng độ chính xác giảm. Model sẽ dựa vào pattern chung của các cửa hàng/sản phẩm tương tự.

### 3. Nếu thiếu thông tin thì sao?

**Trả lời:**
- `total_price` thiếu → Tự động dùng `base_price`
- Các cột khác thiếu → **Không chạy được**, phải điền đầy đủ

### 4. Có cần kết nối internet không?

**Trả lời:** Không. Tất cả chạy offline sau khi đã có đủ files (models + encoders).

### 5. Output có thể sai bao nhiêu?

**Trả lời:** 
- Trường hợp tốt: Sai ~5-10% (dự đoán 50, thực tế 47-53)
- Trường hợp xấu: Sai ~20-30% (outliers, tình huống đặc biệt)
- Trung bình: RMSLE ~360

### 6. Làm sao biết dự đoán đáng tin hay không?

**Các trường hợp dự đoán tin cậy:**
- ✅ Cửa hàng/sản phẩm đã có trong training data
- ✅ Giá không quá cao/thấp bất thường
- ✅ Tuần trong khoảng 2011-2013

**Các trường hợp nên cẩn thận:**
- ⚠️ Cửa hàng/sản phẩm hoàn toàn mới
- ⚠️ Giá giảm/tăng đột ngột (>50%)
- ⚠️ Tuần ngoài khoảng training (trước 2011, sau 2013)

---

## 📋 Tóm tắt nhanh

### Input (Bạn cần chuẩn bị):
- ✅ 1 file CSV với 8 cột thông tin cơ bản
- ✅ Format tuần: DD/MM/YY (ví dụ: 16/07/13)
- ✅ Có sẵn folder `weight/` (10 files)
- ✅ Có sẵn folder `encoders/` (3 files)

### Output (Bạn nhận được):
- ✅ 1 file CSV với 2 cột: record_ID + units_sold
- ✅ Mỗi dòng = 1 dự đoán số lượng bán

### Độ chính xác:
- ✅ RMSLE ~360 (khá tốt)
- ✅ Tốc độ: ~5 giây cho 13,860 dự đoán

---

**END OF GUIDE**

*Tài liệu này được viết cho người dùng không cần background kỹ thuật.*  
*Phiên bản đầy đủ với code: Xem file INFERENCE_GUIDE_TECHNICAL.md*

