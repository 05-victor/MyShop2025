# MyShop.Server Docker Deployment Guide for Render

## Prerequisites

1. **GitHub Repository**: Your code should be pushed to a GitHub repository
2. **Render Account**: Sign up at [render.com](https://render.com)
3. **Database**: You'll need a PostgreSQL database (you can use Render's PostgreSQL service or external like Supabase)

## Quick Deployment Steps

### Option 1: Using Render.yaml (Recommended)

1. **Push your code** to GitHub with the `Dockerfile` and `render.yaml` files
2. **Connect to Render**:
   - Go to [render.com](https://render.com) and sign in
   - Click "New +" ? "Blueprint"
   - Connect your GitHub repository
   - Select the repository containing your MyShop code
3. **Configure Environment Variables** in Render dashboard:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=your-postgres-connection-string
   JwtSettings__SecretKey=your-jwt-secret-key-minimum-32-characters
   EmailSettings__ApiKey=your-encoded-brevo-api-key
   ```
4. **Deploy**: Render will automatically build and deploy your application

### Option 2: Manual Web Service Creation

1. **Create Web Service**:
   - Go to Render dashboard
   - Click "New +" ? "Web Service"
   - Connect your GitHub repository

2. **Configure Service**:
   - **Name**: `myshop-server`
   - **Environment**: `Docker`
   - **Region**: Choose closest to your users (e.g., Oregon)
   - **Branch**: `master` or `main`
   - **Dockerfile Path**: `./Dockerfile`

3. **Set Environment Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   PORT=10000
   ConnectionStrings__DefaultConnection=Host=your-postgres-host;Port=5432;Database=your-db;Username=your-user;Password=your-password;Ssl Mode=Require;Trust Server Certificate=true;
   JwtSettings__SecretKey=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
   JwtSettings__Issuer=MyShop.Server
   JwtSettings__Audience=MyShop.Client
   JwtSettings__ExpiryInMinutes=60
   JwtSettings__RefreshTokenExpiryInDays=7
   EmailSettings__ApiKey=your-encoded-brevo-api-key
   EmailSettings__ApiEndpoint=aHR0cHM6Ly9hcGkuYnJldm8uY29tL3YzL3NtdHAvZW1haWw=
   EmailSettings__SenderName=MyShop
   EmailSettings__SenderEmail=your-sender@email.com
   ```

4. **Deploy**: Click "Create Web Service"

## Environment Variables Guide

### Required Variables

#### Database Connection
```bash
ConnectionStrings__DefaultConnection="Host=your-host;Port=5432;Database=your-db;Username=your-user;Password=your-password;Ssl Mode=Require;Trust Server Certificate=true;"
```

#### JWT Settings
```bash
JwtSettings__SecretKey="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
JwtSettings__Issuer="MyShop.Server"
JwtSettings__Audience="MyShop.Client"
JwtSettings__ExpiryInMinutes="60"
JwtSettings__RefreshTokenExpiryInDays="7"
```

#### Email Settings (if using Brevo)
```bash
EmailSettings__ApiKey="your-base64-encoded-api-key"
EmailSettings__ApiEndpoint="aHR0cHM6Ly9hcGkuYnJldm8uY29tL3YzL3NtdHAvZW1haWw="
EmailSettings__SenderName="MyShop"
EmailSettings__SenderEmail="your-verified-email@domain.com"
```

### Optional Variables
```bash
ASPNETCORE_ENVIRONMENT="Production"
PORT="10000"
```

## Database Setup Options

### Option 1: Render PostgreSQL (Recommended for production)
1. In Render dashboard, click "New +" ? "PostgreSQL"
2. Choose your plan (free tier available)
3. Note the connection details
4. Use the **Internal Connection String** for your app

### Option 2: External Database (Supabase, AWS RDS, etc.)
1. Create PostgreSQL database in your preferred provider
2. Ensure the database allows connections from Render's IP ranges
3. Use the external connection string

## Post-Deployment Steps

### 1. Run Database Migrations
After first deployment, you may need to run migrations:

**Option A: Using Render Shell**
```bash
# In Render dashboard, go to your service ? Shell
dotnet ef database update --no-build
```

**Option B: Add migration commands to Dockerfile**
Add this before the final CMD in Dockerfile:
```dockerfile
# Run migrations on startup (optional)
RUN echo '#!/bin/bash\ndotnet ef database update --no-build\ndotnet MyShop.Server.dll' > /app/start.sh
RUN chmod +x /app/start.sh
CMD ["/app/start.sh"]
```

### 2. Test Your Deployment
```bash
# Health check
curl https://your-app-name.onrender.com/api/health

# Test registration
curl -X POST https://your-app-name.onrender.com/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test123","fullName":"Test User","email":"test@test.com"}'
```

### 3. Monitor Logs
- Go to Render dashboard ? Your service ? Logs
- Monitor for any startup errors or runtime issues

## Docker Build Optimization

The Dockerfile is optimized for:
- **Multi-stage build**: Reduces final image size
- **Layer caching**: Copies project files first for better cache utilization
- **Security**: Runs as non-root user
- **Health checks**: Built-in health monitoring
- **Render compatibility**: Supports PORT environment variable

## Troubleshooting

### Common Issues

1. **Build Failures**:
   - Check if all project references are correct
   - Ensure all NuGet packages are compatible
   - Verify Dockerfile paths are correct

2. **Runtime Errors**:
   - Check environment variables are set correctly
   - Verify database connection string
   - Check logs in Render dashboard

3. **Database Connection Issues**:
   - Ensure database allows external connections
   - Verify SSL requirements in connection string
   - Check if database is in same region (for Render PostgreSQL)

4. **Port Issues**:
   - Render uses PORT environment variable (usually 10000)
   - Don't hardcode ports in your application
   - Health check should use the same port

### Performance Tips

1. **Free Tier Limitations**:
   - Free tier sleeps after 15 minutes of inactivity
   - Cold starts can take 30-60 seconds
   - Consider upgrading for production workloads

2. **Resource Optimization**:
   - Monitor memory usage in Render dashboard
   - Optimize database queries
   - Consider adding Redis for caching

## Security Checklist

- [ ] Use strong JWT secret keys (32+ characters)
- [ ] Enable HTTPS only (Render provides this automatically)
- [ ] Secure database with strong passwords
- [ ] Don't commit secrets to Git
- [ ] Use environment variables for all sensitive data
- [ ] Enable CORS only for trusted domains in production
- [ ] Regularly update dependencies

## Next Steps

1. **Custom Domain**: Add your own domain in Render dashboard
2. **SSL Certificate**: Render provides free SSL certificates
3. **Monitoring**: Set up monitoring and alerting
4. **Backup Strategy**: Regular database backups
5. **CI/CD**: Set up automated deployments on Git push

## Support

- **Render Documentation**: [docs.render.com](https://docs.render.com)
- **Render Community**: [community.render.com](https://community.render.com)
- **Docker Documentation**: [docs.docker.com](https://docs.docker.com)

---

Your MyShop.Server is now ready for production deployment on Render! ??