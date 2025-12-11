# File Upload API - Quick Reference Guide

## Authentication

All file upload endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer YOUR_JWT_TOKEN
```

## Endpoints

### 1. Upload Avatar (Profile Picture)

**Endpoint**: `POST /api/v1/profiles/uploadAvatar`

**Use Case**: Update user's profile picture

**Request**:
```http
POST /api/v1/profiles/uploadAvatar HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="avatar.jpg"
Content-Type: image/jpeg

[binary image data]
------WebKitFormBoundary--
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "result": "https://res.cloudinary.com/dzwtva4p/image/upload/v1762277417/file-service/123312312.jpg",
  "message": "Avatar uploaded successfully",
  "code": 200
}
```

**Behavior**: 
- Validates file type and size
- Uploads to external Cloudinary service
- Automatically updates user's profile with new avatar URL
- Returns Cloudinary URL for immediate use

---

### 2. Upload Product Image

**Endpoint**: `POST /api/v1/products/{productId}/uploadImage`

**Use Case**: Add/update product image

**Request**:
```http
POST /api/v1/products/a1b2c3d4-e5f6-7890-abcd-ef1234567890/uploadImage HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="product.png"
Content-Type: image/png

[binary image data]
------WebKitFormBoundary--
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "result": "https://res.cloudinary.com/dzwtva4p/image/upload/v1762277417/file-service/product_a1b2c3d4.jpg",
  "message": "Product image uploaded successfully",
  "code": 200
}
```

**Behavior**:
- Validates product exists (returns 404 if not found)
- Validates file type and size
- Uploads to external Cloudinary service with identifier "product_{productId}"
- Automatically updates product's imageUrl field
- Returns Cloudinary URL

---

### 3. Generic File Upload

**Endpoint**: `POST /api/v1/files/upload`

**Use Case**: Upload any image file with optional custom identifier

**Request**:
```http
POST /api/v1/files/upload HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="image.jpg"
Content-Type: image/jpeg

[binary image data]
------WebKitFormBoundary
Content-Disposition: form-data; name="identifier"

my-custom-id-123
------WebKitFormBoundary--
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "result": "https://res.cloudinary.com/dzwtva4p/image/upload/v1762277417/file-service/my-custom-id-123.jpg",
  "message": "File uploaded successfully",
  "code": 200
}
```

**Behavior**:
- If no identifier provided, uses a new GUID
- Validates file type and size
- Uploads to external Cloudinary service
- Does NOT update any database records
- Returns Cloudinary URL for manual use

---

## Error Responses

### 400 Bad Request - No File

```json
{
  "success": false,
  "result": null,
  "message": "No file uploaded",
  "code": 400
}
```

### 400 Bad Request - Invalid File Type

```json
{
  "success": false,
  "result": null,
  "message": "File type '.txt' is not allowed. Allowed types: .jpg, .jpeg, .png, .gif, .webp",
  "code": 400
}
```

### 400 Bad Request - File Too Large

```json
{
  "success": false,
  "result": null,
  "message": "File size exceeds maximum allowed size of 5MB",
  "code": 400
}
```

### 401 Unauthorized - Missing/Invalid Token

```json
{
  "success": false,
  "result": null,
  "message": "User not authenticated",
  "code": 401
}
```

### 404 Not Found - Product Doesn't Exist

```json
{
  "success": false,
  "result": null,
  "message": "Product not found",
  "code": 404
}
```

### 500 Internal Server Error - Upload Failed

```json
{
  "success": false,
  "result": null,
  "message": "Failed to upload avatar",
  "code": 500
}
```

---

## Validation Rules

| Rule | Value | Error Code |
|------|-------|------------|
| Max File Size | 5 MB | 400 |
| Allowed Extensions | .jpg, .jpeg, .png, .gif, .webp | 400 |
| Authentication | JWT Bearer Token Required | 401 |
| Product Exists | Must exist in database | 404 |

---

## Testing Examples

### Using cURL

#### 1. Upload Avatar
```bash
curl -X POST https://localhost:5000/api/v1/profiles/uploadAvatar \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@/path/to/avatar.jpg"
```

#### 2. Upload Product Image
```bash
curl -X POST https://localhost:5000/api/v1/products/a1b2c3d4-e5f6-7890-abcd-ef1234567890/uploadImage \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@/path/to/product.png"
```

#### 3. Generic Upload with Custom ID
```bash
curl -X POST https://localhost:5000/api/v1/files/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@/path/to/image.jpg" \
  -F "identifier=custom-id-123"
```

### Using Postman

1. Create a new POST request
2. Set URL to the appropriate endpoint
3. Go to **Headers** tab:
   - Add `Authorization: Bearer YOUR_JWT_TOKEN`
4. Go to **Body** tab:
   - Select **form-data**
   - Add key `file` with type **File**
   - Click "Select Files" and choose your image
   - (Optional for generic upload) Add key `identifier` with type **Text**
5. Click **Send**

### Using JavaScript (Fetch API)

```javascript
const uploadAvatar = async (file, token) => {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('https://localhost:5000/api/v1/profiles/uploadAvatar', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  const result = await response.json();
  return result;
};

// Usage
const fileInput = document.querySelector('#avatar-input');
const file = fileInput.files[0];
const token = 'YOUR_JWT_TOKEN';

uploadAvatar(file, token)
  .then(response => {
    if (response.success) {
      console.log('Avatar URL:', response.result);
    } else {
      console.error('Upload failed:', response.message);
    }
  });
```

---

## Integration Flow

### Avatar Upload Flow

1. User selects image file
2. Frontend calls `POST /api/v1/profiles/uploadAvatar` with file
3. Backend validates file (type, size)
4. Backend uploads to Cloudinary via external service
5. Backend receives Cloudinary URL
6. Backend updates user's Avatar field in database
7. Backend returns Cloudinary URL to frontend
8. Frontend displays new avatar immediately

### Product Image Upload Flow

1. Admin selects product image file
2. Frontend calls `POST /api/v1/products/{productId}/uploadImage` with file
3. Backend validates product exists
4. Backend validates file (type, size)
5. Backend uploads to Cloudinary via external service
6. Backend receives Cloudinary URL
7. Backend updates product's ImageUrl field in database
8. Backend returns Cloudinary URL to frontend
9. Frontend displays new product image

---

## Common Issues & Solutions

### Issue: 401 Unauthorized
**Cause**: Missing or expired JWT token
**Solution**: Ensure you're logged in and using a valid token. Tokens expire after 60 minutes by default.

### Issue: 400 Bad Request - "File type not allowed"
**Cause**: Uploaded file is not an image
**Solution**: Only upload .jpg, .jpeg, .png, .gif, or .webp files

### Issue: 400 Bad Request - "File size exceeds maximum"
**Cause**: File is larger than 5MB
**Solution**: Compress or resize the image before uploading

### Issue: 404 Not Found (Product Upload)
**Cause**: Product ID doesn't exist in database
**Solution**: Verify the product ID is correct and the product hasn't been deleted

### Issue: 500 Internal Server Error
**Cause**: External file service is down or network issues
**Solution**: Check server logs for details. The external service at https://file-service-cdal.onrender.com may be temporarily unavailable.

---

## External Service Details

**Service URL**: `https://file-service-cdal.onrender.com/api/v1/file/uploads`

**Provider**: Cloudinary (via custom wrapper service)

**Storage**: Permanent cloud storage

**CDN**: Automatic content delivery network

**Image URL Format**: `https://res.cloudinary.com/{cloud_name}/image/upload/{version}/{identifier}.{extension}`

---

## Best Practices

1. **Always validate on frontend too**: Don't rely solely on backend validation
2. **Show upload progress**: For better UX, show loading state while uploading
3. **Handle errors gracefully**: Display user-friendly error messages
4. **Compress images**: Reduce file size before upload to improve performance
5. **Use appropriate dimensions**: Resize images to required dimensions before upload
6. **Cache avatar URLs**: Store the URL locally to avoid unnecessary API calls
7. **Lazy load images**: Use lazy loading for better performance

---

## Support

For issues or questions:
1. Check server logs at `MyShop.Server/logs`
2. Verify JWT token is valid
3. Test with Postman first to isolate frontend issues
4. Check external service status: https://file-service-cdal.onrender.com

---

**Last Updated**: January 2025
**Version**: 1.0
