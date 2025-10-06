using MyShop.Shared;
using MyShop.Data;
using Microsoft.EntityFrameworkCore;
using MyShop.Server.GraphQL.Queries;
using MyShop.Server.GraphQL.Mutations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ShopContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), 
                     b => b.MigrationsAssembly("MyShop.Data"))
            .UseSnakeCaseNamingConvention());

// Add GraphQL with HotChocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<UserQueries>()
    .AddMutationType<UserMutations>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map GraphQL endpoint
app.MapGraphQL();

app.Run();
