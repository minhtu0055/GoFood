using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Configurations;
using GoFood.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GoFood.Data.EF
{
    public class GoFoodDbContext : IdentityDbContext<AppUsers>
    {
        public GoFoodDbContext()
        {
        }

        public GoFoodDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=LAPTOP-R3R9CLAI\\SQLEXPRESS;Database=GoFood123;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
        }

        
        public DbSet<CartDetails> CartDetails { get; set; }
        public DbSet<Carts> Carts { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<ProductImage> ProductImage { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Combo> Combo { get; set; }
        public DbSet<ComboProduct> ComboProducts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ComboProductConfiguration());
            builder.ApplyConfiguration(new VoucherConfiguration());
            builder.ApplyConfiguration(new PromotionConfiguration());
            builder.ApplyConfiguration(new PromotionProductConfiguration());
            //builder.ApplyConfiguration(new OrderConfiguration());
            //builder.ApplyConfiguration(new AppUserConfiguration());
            //builder.ApplyConfiguration(new CartConfiguration());
            //builder.ApplyConfiguration(new CartDetailConfiguration());
            //builder.ApplyConfiguration(new ComboConfiguration());
            //builder.ApplyConfiguration(new OrderDetailConfiguration());
            //builder.ApplyConfiguration(new ProductConfiguration());
            //builder.ApplyConfiguration(new ProductImageConfiguration());
        }
    }
}
