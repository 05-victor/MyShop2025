using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;

namespace MyShop.Data
{
    /// <summary>
    /// Database context cho ứng dụng MyShop.
    /// Quản lý kết nối database và cung cấp DbSet cho các entity.
    /// </summary>
    /// <remarks>
    /// Context này chứa:
    /// - DbSet cho tất cả các entity chính (Category, Product, Order, User, Role, Authority)
    /// - Cấu hình relationships giữa các entity
    /// - Cấu hình many-to-many relationships cho User-Role và Role-Authority
    /// </remarks>
    public class ShopContext : DbContext
    {
        /// <summary>
        /// Khởi tạo instance mới của ShopContext.
        /// </summary>
        /// <param name="options">Tùy chọn cấu hình cho DbContext</param>
        public ShopContext(DbContextOptions<ShopContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet cho entity Category - quản lý danh mục sản phẩm.
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// DbSet cho entity Product - quản lý sản phẩm.
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// DbSet cho entity Order - quản lý đơn hàng.
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// DbSet cho entity OrderItem - quản lý chi tiết đơn hàng.
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// DbSet cho entity User - quản lý người dùng.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// DbSet cho entity Role - quản lý vai trò.
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// DbSet cho entity Authority - quản lý quyền hạn.
        /// </summary>
        public DbSet<Authority> Authorities { get; set; }

        /// <summary>
        /// Cấu hình model và relationships khi tạo database.
        /// </summary>
        /// <param name="modelBuilder">Builder để cấu hình model</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình many-to-many relationship User-Role
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(j => j.ToTable("user_roles")); // Custom table name

            // Cấu hình many-to-many relationship Role-Authority
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Authorities)
                .WithMany(a => a.Roles)
                .UsingEntity(j => j.ToTable("role_authorities")); // Custom table name

            // Cấu hình unique constraint cho User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cấu hình relationship Order-User
            //modelBuilder.Entity<Order>()
            //    .HasOne<User>()
            //    .WithMany(u => u.Orders)
            //    .HasForeignKey("UserId")
            //    .IsRequired(false);

            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Name = "Admin" },
                new Role { Name = "SalesAgent" }
            );

            // Seed authorities
            modelBuilder.Entity<Authority>().HasData(
                new Authority { Name = "POST" },
                new Authority { Name = "DELETE" },
                new Authority { Name = "ALL" }
                //new Authority { Name = "ManageProducts" },
                //new Authority { Name = "ManageOrders" },
                //new Authority { Name = "ViewReports" }
            );

            //TODO: Seed role-authority relationships (have to explicitly create join table entity)


            // Cấu hình table names theo convention
            //modelBuilder.Entity<Category>().ToTable("Categories");
            //modelBuilder.Entity<Product>().ToTable("Products");
            //modelBuilder.Entity<Order>().ToTable("Orders");
            //modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            //modelBuilder.Entity<User>().ToTable("Users");
            //modelBuilder.Entity<Role>().ToTable("Roles");
            //modelBuilder.Entity<Authority>().ToTable("Authorities");
        }
    }
}