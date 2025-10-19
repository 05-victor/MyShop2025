# API Configuration Guide

## Current Configuration

The client is configured to connect to the backend server at:
```json
{
  "BaseUrl": "https://localhost:7120"
}
```

Location: `src\MyShop.Client\ApiServer\ApiConfig.json`

---

## Troubleshooting Connection Issues

### Error: "No connection could be made because the target machine actively refused it"

**Cause:** The backend server is not running or not listening on port 7120.

**Solutions:**

#### 1. Verify Server is Running
```bash
# Navigate to server directory
cd src\MyShop.Server

# Run the server
dotnet run
```

The server should display:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7120
      Now listening on: http://localhost:5000
```

#### 2. Check Server Port Configuration
Open `src\MyShop.Server\Properties\launchSettings.json`:

```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "applicationUrl": "https://localhost:7120;http://localhost:5000"
    }
  }
}
```

#### 3. Verify Firewall/Antivirus
- Ensure Windows Firewall allows connections to localhost:7120
- Check if antivirus is blocking the connection

#### 4. Test Server Manually
Open browser and navigate to:
```
https://localhost:7120/swagger
```

You should see the Swagger API documentation page.

---

## Starting Both Client and Server

### Option 1: Visual Studio (Recommended)
1. Set Multiple Startup Projects:
   - Right-click solution → Properties
   - Select "Multiple startup projects"
   - Set both `MyShop.Server` and `MyShop.Client` to "Start"

2. Press F5 to run both projects

### Option 2: Command Line
**Terminal 1 (Server):**
```bash
cd src\MyShop.Server
dotnet run
```

**Terminal 2 (Client):**
```bash
cd src\MyShop.Client
dotnet run
```

### Option 3: VS Code
Use the provided launch configurations in `.vscode/launch.json`:
1. Select "Launch Server" from debug dropdown
2. Start debugging (F5)
3. Open new VS Code window for client
4. Select "Launch Client" and start debugging

---

## Changing the Server Port

If you need to use a different port:

### 1. Update Server Configuration
Edit `src\MyShop.Server\Properties\launchSettings.json`:
```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:YOUR_PORT;http://localhost:5000"
    }
  }
}
```

### 2. Update Client Configuration
Edit `src\MyShop.Client\ApiServer\ApiConfig.json`:
```json
{
  "BaseUrl": "https://localhost:YOUR_PORT"
}
```

### 3. Rebuild Both Projects
```bash
dotnet clean
dotnet build
```

---

## SSL Certificate Issues

### Error: "The SSL connection could not be established"

**Solution:**
```bash
# Trust the development certificate
dotnet dev-certs https --trust
```

Click "Yes" when prompted to trust the certificate.

---

## Network Diagnostics

### Test if Port is in Use
```powershell
# Windows PowerShell
netstat -ano | findstr :7120
```

### Test Connection
```powershell
# Windows PowerShell
Test-NetConnection -ComputerName localhost -Port 7120
```

---

## Common Scenarios

### Scenario 1: Port Already in Use
**Error:** "Failed to bind to address https://localhost:7120"

**Solution:**
1. Find process using the port:
   ```powershell
   netstat -ano | findstr :7120
   ```
2. Kill the process:
   ```powershell
   taskkill /PID [process_id] /F
   ```

### Scenario 2: Server Starts but Client Can't Connect
**Checklist:**
- [ ] Server is running and shows "Now listening on: https://localhost:7120"
- [ ] ApiConfig.json has correct URL
- [ ] SSL certificate is trusted
- [ ] No firewall blocking the connection
- [ ] Client was rebuilt after changing ApiConfig.json

### Scenario 3: Connection Works Sometimes
**Possible Causes:**
- Server restarting automatically (file changes triggering hot reload)
- Network timeout (increase timeout in API configuration)
- Server under heavy load (check server logs)

---

## Environment-Specific Configuration

For different environments, you can create multiple config files:

### Development
`ApiConfig.Development.json`:
```json
{
  "BaseUrl": "https://localhost:7120"
}
```

### Staging
`ApiConfig.Staging.json`:
```json
{
  "BaseUrl": "https://staging-api.myshop.com"
}
```

### Production
`ApiConfig.Production.json`:
```json
{
  "BaseUrl": "https://api.myshop.com"
}
```

---

## API Health Check

Create a simple health check endpoint on the server:

```csharp
// In Program.cs or a controller
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));
```

Test from client:
```csharp
var response = await _httpClient.GetAsync("/health");
if (response.IsSuccessStatusCode) {
    // Server is reachable
}
```

---

## Debugging Tips

### Enable Detailed Logging
In `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### Monitor Network Traffic
Use tools like:
- Fiddler
- Postman
- Browser DevTools (for REST APIs)

### Check Server Logs
Look for startup errors or exceptions in the server console output.

---

## Quick Reference

| Issue | Check |
|-------|-------|
| Connection refused | Server running? Port correct? |
| SSL error | Certificate trusted? |
| Timeout | Server responding? Network stable? |
| 404 errors | API endpoint correct? Route configured? |
| 401/403 errors | Authentication working? Token valid? |

---

## Support

If issues persist:
1. Check server logs for errors
2. Verify network connectivity
3. Test with Postman/curl to isolate client vs server issues
4. Review recent code changes
5. Check Windows Event Viewer for system-level errors
