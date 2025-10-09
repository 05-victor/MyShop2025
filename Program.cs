using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Data.Entities;

Console.WriteLine("Testing database connection...");

var connectionString = "Host=aws-1-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.jyscmxyfqevdgiqdoalh;Password=nhbtiZiHUci3tLYp;Ssl Mode=Require;Trust Server Certificate=true;";

var options = new DbContextOptionsBuilder<ShopContext>()
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention()
    .Options;

using var context = new ShopContext(options);

try
{
    // Test connection
    await context.Database.CanConnectAsync();
    Console.WriteLine("? Database connection successful!");

    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("? Database tables ensured!");

    // Create sample authorities if not exist
    var authorities = new[]
    {
        new Authority { Name = "USER_READ", Description = "??c thông tin ng??i dùng" },
        new Authority { Name = "USER_WRITE", Description = "Ghi/s?a thông tin ng??i dùng" },
        new Authority { Name = "PRODUCT_READ", Description = "??c thông tin s?n ph?m" },
        new Authority { Name = "PRODUCT_WRITE", Description = "Ghi/s?a thông tin s?n ph?m" }
    };

    foreach (var auth in authorities)
    {
        var exists = await context.Authorities.AnyAsync(a => a.Name == auth.Name);
        if (!exists)
        {
            context.Authorities.Add(auth);
            Console.WriteLine($"? Added authority: {auth.Name}");
        }
        else
        {
            Console.WriteLine($"- Authority already exists: {auth.Name}");
        }
    }

    await context.SaveChangesAsync();
    Console.WriteLine("? Sample data created successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"? Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
    }
}