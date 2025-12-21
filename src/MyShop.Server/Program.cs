using MyShop.Shared;
using MyShop.Data;
using Microsoft.EntityFrameworkCore;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Server.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using MyShop.Data.Repositories.Implementations;
using AutoMapper;
using MyShop.Server.Factories.Implementations;
using MyShop.Server.Factories.Interfaces;
using MyShop.Server.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configuration is automatically loaded from appsettings.json
// No need to configure separate settings classes

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add JWT Authentication
// Read JWT settings directly from configuration
var secretKey = builder.Configuration["JwtSettings:SecretKey"];
var issuer = builder.Configuration["JwtSettings:Issuer"];
var audience = builder.Configuration["JwtSettings:Audience"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:SecretKey is not properly configured. It must be at least 32 characters long.");
}

if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    throw new InvalidOperationException("JwtSettings:Issuer and Audience must be configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // Disable default claim mapping
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero, // Remove default 5 minute clock skew
        NameClaimType = ClaimTypes.Name, // Set the Name claim type
        RoleClaimType = "role"         // Match MapInboundClaims = false
    };

    // Configure JWT events for debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("❌ Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var username = context.Principal?.Identity?.Name;
            logger.LogInformation("✅ Token validated for user: {User}", username);
            
            // Debug: Log all claims in the token
            if (context.Principal != null)
            {
                logger.LogInformation("📋 Token claims:");
                foreach (var claim in context.Principal.Claims)
                {
                    logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
                }
            }
            
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add DbContext
builder.Services.AddDbContext<ShopContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), 
                     b => b.MigrationsAssembly("MyShop.Data"))
            .UseSnakeCaseNamingConvention());

// Add Factory
builder.Services.AddScoped<IProductFactory, ProductFactory>();
builder.Services.AddScoped<IOrderFactory, OrderFactory>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IAgentRequestRepository, AgentRequestRepository>();

// Register Mappers
builder.Services.AddScoped<MyShop.Server.Mappings.CartMapper>();
// Register Services
builder.Services.AddHttpClient<IUserService, UserService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserAuthorityService, UserAuthorityService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAgentRequestService, AgentRequestService>();
builder.Services.AddScoped<IEarningsService, EarningsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Register HttpClient for EmailNotificationService
builder.Services.AddHttpClient<IEmailNotificationService, EmailNotificationService>();

// Register HttpClient and FileUploadService
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ✅ IMPORTANT: Add Global Exception Handler FIRST (before other middleware)
app.UseGlobalExceptionHandler();

// ✅ Add connection logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var connectionId = context.Connection.Id;
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    
    logger.LogInformation("✅ Client connected: {ConnectionId} from {ClientIP}", connectionId, clientIp);
    
    try
    {
        await next();
    }
    finally
    {
        logger.LogInformation("⚠️ Client disconnected: {ConnectionId} from {ClientIP}", connectionId, clientIp);
    }
});

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
