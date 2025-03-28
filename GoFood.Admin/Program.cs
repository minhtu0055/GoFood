using GoFood.ApiIntegration.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
namespace GoFood.Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Index";
                    options.AccessDeniedPath = "/User/Forbidden/";
                });
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddTransient<IUserApiClient, UserApiClient>();
            builder.Services.AddTransient<IRoleApiClient, RoleApiClient>();
            builder.Services.AddTransient<IComboApiClient, ComboApiClient>();
            builder.Services.AddTransient<IProductApiClient, ProductApiClient>();
            builder.Services.AddTransient<ICategoryApiClient, CategoryApiClient>();
            builder.Services.AddTransient<IOrderApiClient, OrderApiClient>();
            builder.Services.AddTransient<IPromotionApiClient, PromotionApiClient>();
            builder.Services.AddTransient<IVoucherApiClient, VoucherApiClient>();
            builder.Services.AddTransient<IVoucherApiClient, VoucherApiClient>();
            builder.Services.AddRazorPages();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
