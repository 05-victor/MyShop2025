using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;

namespace MyShop.Shared
{
    public class ShopContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }   // Table "categories"

        public DbSet<Product> Products { get; set; }      // Table "products"

        public DbSet<Order> Orders { get; set; }          // Table "orders"

        public DbSet<OrderItem> OrderItems { get; set; }  // Table "order_items"

        public DbSet<User> Users { get; set; }            // Table "users"

        // Constructor
        public ShopContext(DbContextOptions<ShopContext> options) : base(options)
        {
        }

        // Configure the DbContext for PostgreSQL
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        //optionsBuilder
        //            //.UseNpgsql("Host=localhost;Database=mydatabase;Username=myuser;Password=mypassword");
        //            //.UseSnakeCaseNamingConvention();
        //    }
        //}
    }
}
