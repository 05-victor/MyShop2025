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
        /// DbSet cho entity RoleAuthorities - quản lý mối quan hệ giữa Role và Authority.
        /// </summary>
        public DbSet<RoleAuthorities> RoleAuthorities { get; set; }

        /// <summary>
        /// DbSet cho entity RemovedAuthorities - quản lý quyền hạn bị loại bỏ của từng user.
        /// </summary>
        public DbSet<RemovedAuthorities> RemovedAuthorities { get; set; }

        public DbSet<Profile> Profiles { get; set; }

        /// <summary>
        /// DbSet cho entity CartItem - quản lý giỏ hàng.
        /// </summary>
        public DbSet<CartItem> CartItems { get; set; }
        
        /// <summary>
        /// DbSet cho entity AgentRequest - quản lý yêu cầu trở thành sales agent.
        /// </summary>
        public DbSet<AgentRequest> AgentRequests { get; set; }

        /// <summary>
        /// DbSet cho entity RefreshToken - quản lý refresh tokens.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Cấu hình model và relationships khi tạo database.
        /// </summary>
        /// <param name="modelBuilder">Builder để cấu hình model</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure enum properties to be stored as integers
            ConfigureEnums(modelBuilder);

            // Cấu hình many-to-many relationship User-Role
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(j => j.ToTable("user_roles")); // Custom table name

            // Cấu hình many-to-many relationship Role-Authority P/s: doesn't need because of explicit RoleAuthorities entity
            //modelBuilder.Entity<Role>()
            //    .HasMany(r => r.Authorities)
            //    .WithMany(a => a.Roles)
            //    .UsingEntity(j => j.ToTable("role_authorities")); // Custom table name

            // Cấu hình unique constraint cho User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Profile configuration
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                
                entity.HasOne(p => p.User)
                    .WithOne(u => u.Profile)
                    .HasForeignKey<Profile>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(p => p.UserId).IsUnique();
            });

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
            // --- Relationship setup ---
            modelBuilder.Entity<RoleAuthorities>()
                .HasKey(ra => new { ra.RoleName, ra.AuthorityName });

            modelBuilder.Entity<RoleAuthorities>()
                .HasOne(ra => ra.Role)
                .WithMany(r => r.RoleAuthorities)
                .HasForeignKey(ra => ra.RoleName);

            modelBuilder.Entity<RoleAuthorities>()
                .HasOne(ra => ra.Authority)
                .WithMany(a => a.RoleAuthorities)
                .HasForeignKey(ra => ra.AuthorityName);

            // Optional: rename the join table
            modelBuilder.Entity<RoleAuthorities>()
                .ToTable("role_authorities");

            // Seed data for RoleAuthorities
            modelBuilder.Entity<RoleAuthorities>()
                .HasData(
                    new RoleAuthorities { RoleName = "Admin", AuthorityName = "ALL" },
                    new RoleAuthorities { RoleName = "SalesAgent", AuthorityName = "POST" }
                );

            // --- RemovedAuthorities Configuration ---
            modelBuilder.Entity<RemovedAuthorities>()
                .HasKey(ra => new { ra.UserId, ra.AuthorityName });

            modelBuilder.Entity<RemovedAuthorities>()
                .HasOne(ra => ra.User)
                .WithMany(u => u.RemovedAuthorities)
                .HasForeignKey(ra => ra.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RemovedAuthorities>()
                .HasOne(ra => ra.Authority)
                .WithMany()
                .HasForeignKey(ra => ra.AuthorityName)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RemovedAuthorities>()
                .ToTable("removed_authorities");

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);

                entity.HasIndex(ci => new { ci.UserId, ci.ProductId })
                    .IsUnique();

                entity.HasOne(ci => ci.User)
                    .WithMany()
                    .HasForeignKey(ci => ci.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                    .WithMany()
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // AgentRequest configuration
            modelBuilder.Entity<AgentRequest>(entity =>
            {
                entity.HasKey(ar => ar.Id);
                
                entity.HasIndex(ar => ar.UserId);
                entity.HasIndex(ar => ar.Status);
                entity.HasIndex(ar => ar.RequestedAt);
                
                entity.HasOne(ar => ar.User)
                    .WithMany()
                    .HasForeignKey(ar => ar.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.ExpiresAt);
                entity.HasIndex(rt => rt.RevokedAt);
                
                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        /// <summary>
        /// Configure enum properties to be stored as integers in the database
        /// </summary>
        private void ConfigureEnums(ModelBuilder modelBuilder)
        {
            // Product.Status -> integer
            modelBuilder.Entity<Product>()
                .Property(p => p.Status)
                .HasConversion<int>();

            // Order.Status -> integer
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<int>();

            // Order.PaymentStatus -> integer
            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentStatus)
                .HasConversion<int>();

            // AgentRequest.Status -> integer
            modelBuilder.Entity<AgentRequest>()
                .Property(ar => ar.Status)
                .HasConversion<int>();
        }
    }
}