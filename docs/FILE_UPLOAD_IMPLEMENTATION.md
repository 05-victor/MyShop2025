# File Upload Feature Implementation

## Overview

This document describes the implementation of the file upload feature for MyShop.Server backend, which integrates with an external file upload service hosted on Render.

## External Service

**Endpoint**: `https://file-service-cdal.onrender.com/api/v1/file/uploads`

**Method**: POST (multipart/form-data)

**Request Parameters**:
- `id` (text): Identifier for the file (e.g., userId, productId)
- `image` (file): The image file to upload

**Response Format**:
```json
{
  "code": 1000,
  "message": "Upload successful",
  "result": {
    "image": "https://res.cloudinary.com/..."
  }
}
```

## Implementation

### 1. File Upload Service

**Interface**: `IFileUploadService`
- Location: `src/MyShop.Server/Services/Interfaces/IFileUploadService.cs`
- Method: `Task<string> UploadImageAsync(IFormFile file, string identifier)`

**Implementation**: `FileUploadService`
- Location: `src/MyShop.Server/Services/Implementations/FileUploadService.cs`
- Features:
  - File type validation (jpg, jpeg, png, gif, webp)
  - File size validation (max 5MB)
  - Integration with external Render-hosted file service
  - Proper error handling and logging

### 2. API Endpoints

#### Profile Avatar Upload

**Endpoint**: `POST /api/v1/profiles/uploadAvatar`

**Authentication**: Required (JWT Bearer token)

**Request**: multipart/form-data
- `file`: Image file

**Response**:
```json
{
  "success": true,
  "result": "https://res.cloudinary.com/...",
  "message": "Avatar uploaded successfully",
  "code": 200
}
```

**Example using curl**:
```bash
curl -X POST https://your-server.com/api/v1/profiles/uploadAvatar \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@avatar.jpg"
```

#### Product Image Upload

**Endpoint**: `POST /api/v1/products/{id}/uploadImage`

**Authentication**: Required (JWT Bearer token)

**Request**: multipart/form-data
- `file`: Image file

**Response**:
```json
{
  "success": true,
  "result": "https://res.cloudinary.com/...",
  "message": "Product image uploaded successfully",
  "code": 200
}
```

**Example using curl**:
```bash
curl -X POST https://your-server.com/api/v1/products/123e4567-e89b-12d3-a456-426614174000/uploadImage \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@product.jpg"
```

#### Generic File Upload

**Endpoint**: `POST /api/v1/files/upload`

**Authentication**: Required (JWT Bearer token)

**Request**: multipart/form-data
- `file`: Image file
- `identifier` (optional): Custom identifier (defaults to new GUID)

**Response**:
```json
{
  "success": true,
  "result": "https://res.cloudinary.com/...",
  "message": "File uploaded successfully",
  "code": 200
}
```

**Example using curl**:
```bash
curl -X POST https://your-server.com/api/v1/files/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@image.jpg" \
  -F "identifier=custom-id-123"
```

### 3. Controllers

#### ProfileController
- Location: `src/MyShop.Server/Controllers/ProfileController.cs`
- New endpoint: `uploadAvatar`
- Automatically updates user profile with new avatar URL

#### ProductController
- Location: `src/MyShop.Server/Controllers/ProductController.cs`
- New endpoint: `{id}/uploadImage`
- Automatically updates product with new image URL

#### FileController
- Location: `src/MyShop.Server/Controllers/FileController.cs`
- Generic file upload endpoint for other use cases

### 4. Dependency Registration

Location: `src/MyShop.Server/Program.cs`

```csharp
// Register HttpClient and FileUploadService
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
```

## Validation Rules

1. **File Type**: Only image files are allowed
   - Supported: .jpg, .jpeg, .png, .gif, .webp
   - Others: Rejected with 400 Bad Request

2. **File Size**: Maximum 5MB
   - Larger files: Rejected with 400 Bad Request

3. **Authentication**: All endpoints require valid JWT token
   - Missing/Invalid token: Rejected with 401 Unauthorized

## Error Handling

All endpoints return standardized error responses:

```json
{
  "success": false,
  "result": null,
  "message": "Error message here",
  "code": 400/401/404/500
}
```

Common error scenarios:
- **400 Bad Request**: Invalid file type, file too large, no file uploaded
- **401 Unauthorized**: Missing or invalid JWT token
- **404 Not Found**: Product not found (for product image upload)
- **500 Internal Server Error**: External service failure, network issues

## Logging

The FileUploadService logs the following events:
- File upload initiated (with filename and identifier)
- File upload success (with resulting URL)
- File upload failure (with error details)

Log level:
- `Information`: Successful uploads
- `Warning`: Validation failures
- `Error`: Upload failures, external service errors

## Testing

### Using Postman

1. **Avatar Upload**:
   - Method: POST
   - URL: `http://localhost:5000/api/v1/profiles/uploadAvatar`
   - Headers: `Authorization: Bearer YOUR_JWT_TOKEN`
   - Body: form-data
     - Key: `file`, Type: File, Value: Select image file

2. **Product Image Upload**:
   - Method: POST
   - URL: `http://localhost:5000/api/v1/products/{productId}/uploadImage`
   - Headers: `Authorization: Bearer YOUR_JWT_TOKEN`
   - Body: form-data
     - Key: `file`, Type: File, Value: Select image file

3. **Generic Upload**:
   - Method: POST
   - URL: `http://localhost:5000/api/v1/files/upload`
   - Headers: `Authorization: Bearer YOUR_JWT_TOKEN`
   - Body: form-data
     - Key: `file`, Type: File, Value: Select image file
     - Key: `identifier`, Type: Text, Value: "custom-id" (optional)

## Future Enhancements

1. **Multiple File Upload**: Support uploading multiple files at once
2. **Image Resizing**: Automatically resize images to optimal sizes
3. **Thumbnail Generation**: Generate thumbnails for product images
4. **File Type Expansion**: Support documents (PDF, DOCX) for other features
5. **Progress Tracking**: Add upload progress reporting for large files
6. **Direct Storage**: Consider implementing direct cloud storage (AWS S3, Azure Blob)
7. **Caching**: Cache uploaded URLs to reduce external API calls

## Security Considerations

1. All endpoints are protected with JWT authentication
2. File type validation prevents malicious file uploads
3. File size limit prevents DoS attacks
4. External service handles actual file storage (isolated from main database)
5. No file content is stored on the server (passed directly to external service)

## Dependencies

- `Microsoft.AspNetCore.Http.Features` (for IFormFile)
- `System.Net.Http` (for HttpClient)
- `System.Text.Json` (for parsing external service response)

## Related Files

- `src/MyShop.Server/Services/Interfaces/IFileUploadService.cs`
- `src/MyShop.Server/Services/Implementations/FileUploadService.cs`
- `src/MyShop.Server/Controllers/ProfileController.cs`
- `src/MyShop.Server/Controllers/ProductController.cs`
- `src/MyShop.Server/Controllers/FileController.cs`
- `src/MyShop.Server/Program.cs`

## Notes

- The implementation focuses on **backend only** as requested
- No changes were made to the frontend (MyShop.Client)
- The external file service URL is hardcoded but can be moved to appsettings.json for configuration
- All uploaded files are stored on Cloudinary via the external service
