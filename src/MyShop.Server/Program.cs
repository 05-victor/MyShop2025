using MyShop.Shared;
using MyShop.Data;
using Microsoft.EntityFrameworkCore;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Server.Services.Implementations;
using MyShop.Server.Configuration;
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

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JWT settings are not properly configured");
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero, // Remove default 5 minute clock skew
        NameClaimType = ClaimTypes.Name, // Set the Name claim type
        //NameClaimType = "unique_name", // Match MapInboundClaims = false
        //RoleClaimType = ClaimTypes.Role // Set the Role claim type
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
