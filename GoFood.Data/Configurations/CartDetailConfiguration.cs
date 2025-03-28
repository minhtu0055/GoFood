using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoFood.Data.Configurations
{
    public class CartDetailConfiguration : IEntityTypeConfiguration<CartDetails>
    {
        public void Configure(EntityTypeBuilder<CartDetails> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x=>x.Carts).WithMany(x=>x.CartDetails).HasForeignKey(x=>x.CartId);

            builder.HasOne(x=>x.Products).WithMany(x=>x.CartDetails).HasForeignKey(x=>x.ProductId);
            
        }
    }
}
