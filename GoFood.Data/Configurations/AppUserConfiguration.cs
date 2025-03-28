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
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUsers>
    {
        public void Configure(EntityTypeBuilder<AppUsers> builder)
        {
            builder.Property(x=>x.FirstName).IsRequired().HasMaxLength(200);        
            builder.Property(x=>x.LastName).IsRequired().HasMaxLength(200);        
            builder.Property(x=>x.UserName).IsRequired().HasMaxLength(200);        
            builder.Property(x=>x.PasswordHash).IsRequired().HasMaxLength(200);        
            builder.Property(x=>x.DateOfBirth).IsRequired();
            builder.HasOne(x => x.Carts).WithOne(x => x.User).HasForeignKey<Carts>(x => x.UserId);
        }
    }
}
