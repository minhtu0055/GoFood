using System.IO.Enumeration;
using GoFood.Application.Catalog.Category;
using GoFood.Application.Catalog.Orders;
using GoFood.Application.Catalog.Products;
using GoFood.Application.Catalog.Promotions;
using GoFood.Application.Catalog.Vouchers;
using GoFood.Application.Common;
using GoFood.Application.System.Roles;
using GoFood.Application.System.Users;
using GoFood.Data.EF;
using GoFood.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using GoFood.Utilities;
using GoFood.Application.Catalog.Combo;

namespace GoFood.BackendApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<GoFoodDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddIdentity<AppUsers,IdentityRole>(options =>
            {
                options.Password.RequireDigit = true; //yêu có ít nhất 1 chữ số 
                options.Password.RequireLowercase = true; // yêu cầu có ít nhất chứ thường
                options.Password.RequireUppercase = true; // yêu cầu có ít nhất một chữ hoa
                options.Password.RequireNonAlphanumeric = true; // yêu cầu có ít nhất một ký tự đặc biệt

                options.User.RequireUniqueEmail = true; // yêu cầu mỗi email phải là duy nhất, không thể hai toàn khoản có trùng email
                options.Lockout.MaxFailedAccessAttempts = 5; // sau 5 lần nhập sai mật khẩu 
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // khóa tài khoản trong 5 phút
            }).AddEntityFrameworkStores<GoFoodDbContext>() // sử dụng addentity để lưu trữ dữ liệu người dùng vào database
            .AddDefaultTokenProviders(); // cung cấp các token cho tính năng: xác thực          
            //Declare DI
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IRoleService, RoleService>();
            builder.Services.AddTransient<ICategoryService, CategoryService>();
            builder.Services.AddTransient<IStorageService, FileStorageService>();
            builder.Services.AddTransient<IProductService, ProductService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IPromotionService, PromotionService>();
            builder.Services.AddTransient<IVoucherService, VoucherService>();
            builder.Services.AddTransient<IOrderService, OrderService>();
            builder.Services.AddTransient<IComboService, ComboService>();
            builder.Services.AddTransient<UserManager<AppUsers>, UserManager<AppUsers>>();
            builder.Services.AddTransient<SignInManager<AppUsers>, SignInManager<AppUsers>>();
            builder.Services.AddTransient<RoleManager<IdentityRole>, RoleManager<IdentityRole>>();
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(x =>
            {       //Thêm bảo mật có JWT Cho swagger
                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Nhập 'Bearer' [dấu cách] rồi token của bạn vào ô bên dưới.\r\n\r\nVí dụ: Bearer 12345abcdef",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                // Bắt buộc Swagger yêu cầu JWT Token khi gọi API
                // nếu token không hợp lệ sẽ không được phép gọi API
                x.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }        
                });
            });
            string issuer = builder.Configuration.GetValue<string>("Tokens:Issuer");
            string signingKey = builder.Configuration.GetValue<string>("Tokens:Key");
            byte[] signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(signingKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = issuer,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = System.TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
                };
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Serve static files from wwwroot
            app.UseStaticFiles();

            // Serve static files from user-content folder
            var userContentPath = Path.Combine(builder.Environment.ContentRootPath, "user-content");
            if (!Directory.Exists(userContentPath))
            {
                Directory.CreateDirectory(userContentPath);
                Directory.CreateDirectory(Path.Combine(userContentPath, "images"));
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(userContentPath),
                RequestPath = "/user-content"
            });

            app.UseAuthentication();
            app.UseAuthorization();

            
            app.MapControllers();

            app.Run();
        }
    }
}
