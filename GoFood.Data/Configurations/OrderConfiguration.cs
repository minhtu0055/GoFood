using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using GoFood.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoFood.Data.Configurations
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x=>x.Id);
            builder.HasOne(x=>x.AppUsers).WithMany(x=>x.Orders).HasForeignKey(x=>x.UserId);
            builder.Property(x=>x.PhoneNumber).IsRequired();
            builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            
            // Configure Voucher relationship
            builder.HasOne(x => x.Voucher)
                .WithMany(x => x.Order)
                .HasForeignKey(x => x.VoucherId)
                .IsRequired(false); // Optional relationship
        }
    }
}
