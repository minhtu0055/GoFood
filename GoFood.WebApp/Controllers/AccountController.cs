using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GoFood.ApiIntegration.Services;
using GoFood.Utilities.Constants;
using GoFood.ViewModels.System.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GoFood.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserApiClient _userApiClient; 
        private readonly IConfiguration _configuration;

        public AccountController(IUserApiClient userApiClient, IConfiguration configuration)
        {
            _userApiClient = userApiClient;
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if(!ModelState.IsValid)
            {
                return View(ModelState);
            }
            var result = await _userApiClient.Authenticate(request);
            if (result.ResultObj == null)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }
            var userPrincipal = this.ValidateToken(result.ResultObj);
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = request.RememberMe
            };
            HttpContext.Session.SetString(SystemConstants.AppSettings.Token, result.ResultObj);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userPrincipal,
                authProperties);
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {         
            if (!ModelState.IsValid)
            {
                return View(request);
            }           
            var result = await _userApiClient.Register(request);
            if (!result.IsSuccessed)
            {
                ModelState.AddModelError("", result.Message);
                return View(request);
            }
            var loginResult = await _userApiClient.Authenticate(new LoginRequest()
            {
                UserName = request.UserName,
                Password = request.Password,
                RememberMe = true
            });

            var userPrincipal = this.ValidateToken(loginResult.ResultObj);
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                IsPersistent = false
            };
            HttpContext.Session.SetString(SystemConstants.AppSettings.Token, loginResult.ResultObj);
            await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        userPrincipal,
                        authProperties);

            return RedirectToAction("Index", "Home");
        }
        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            // Hiển thị thông tin chi tiết lỗi (PII) để dễ dàng gỡ lỗi
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken; // Khai báo biến để lưu trữ token đã được xác thực
            TokenValidationParameters validationParameters = new TokenValidationParameters(); // Tạo đối tượng chứa các tham số xác thực token
            validationParameters.ValidateLifetime = true; // Xác thực thời gian sống của token

            // Thiết lập các tham số xác thực
            validationParameters.ValidAudience = _configuration["Tokens:Issuer"]; // Đặt giá trị Audience hợp lệ từ cấu hình
            validationParameters.ValidIssuer = _configuration["Tokens:Issuer"]; // Đặt giá trị Issuer hợp lệ từ cấu hình
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"])); // Đặt khóa ký token từ cấu hình

            // Xác thực token và lấy ClaimsPrincipal
            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);
            return principal; // Trả về ClaimsPrincipal sau khi xác thực token
        }
    }
}
