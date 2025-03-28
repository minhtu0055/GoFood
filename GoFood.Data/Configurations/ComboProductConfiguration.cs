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
    public class ComboProductConfiguration : IEntityTypeConfiguration<ComboProduct>
    {
        public void Configure(EntityTypeBuilder<ComboProduct> builder)
        {
            builder.HasKey(x => new {x.ProductId , x.ComboId});
            builder.HasOne(x=>x.Combo).WithMany(x=>x.ComboProducts).HasForeignKey(x=>x.ComboId);
            builder.HasOne(x=>x.Products).WithMany(x=>x.ComboProducts).HasForeignKey(x=>x.ProductId);
        }
    }
}
