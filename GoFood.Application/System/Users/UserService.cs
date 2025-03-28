using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Entities;
using GoFood.Utilities;
using GoFood.ViewModels.Common;
using GoFood.ViewModels.System.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;



namespace GoFood.Application.System.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUsers> _userManager;
        private readonly SignInManager<AppUsers> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;

        public UserService(UserManager<AppUsers> userManager, SignInManager<AppUsers> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = configuration;
            _emailSender = emailSender;
        }

        public async Task<ApiResult<string>> Authenticate(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName); // kiểm tra người dùng có tồn tại trong hệ thống hay không
            if (user == null) return new ApiErrorResult<string>("Tài khoản không tồn tại");
            if (!user.IsActive) // kiểm tra trạng thái tài khoản
            {
                return new ApiErrorResult<string>("Tài khoản đã bị hủy kích hoạt");
            }
            var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true); // kiểm tra mật khẩu có đúng hay không
            if (!result.Succeeded)
            {
                return new ApiErrorResult<string>("Đăng nhập không đúng");
            }
            var roles = await _userManager.GetRolesAsync(user); // lấy danh sách quyền roles của người dùng
            // Tạo danh sách claims (thông tin nhúng vào token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Role, string.Join(";",roles)),
                new Claim(ClaimTypes.Name, request.UserName)
            };
            // Tạo một khóa đối xứng SymmetricSecurityKey là cùng một khóa được dùng để ký và xác minh jwt
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"])); // chuyển đổi khóa bí mật thành mảng byte[], jwt yều cầu khóa dưới dạng byte[], không phải string
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                _config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds);
            return new ApiSuccessResult<string>(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<ApiResult<bool>> Register(RegisterRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user != null)
            {
                return new ApiErrorResult<bool>("Tài khoản đã tồn tại");
            }
            user = new AppUsers()
            {
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return new ApiSuccessResult<bool>();
            }
            return new ApiErrorResult<bool>("Đăng ký không thành công");
        }
        public async Task<ApiResult<PagedResult<UserViewModels>>> GetUsersPaging(GetUserPagingRequest request)
        {
            var query = _userManager.Users;
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.UserName.Contains(request.Keyword)
                 || x.PhoneNumber.Contains(request.Keyword));
            }

            //3. Paging
            int totalRow = await query.CountAsync();

            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new UserViewModels()
                {
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    UserName = x.UserName,
                    FirstName = x.FirstName,
                    Id = x.Id,
                    LastName = x.LastName,
                    IsActive = x.IsActive,
                }).ToListAsync();

            //4. Select and projection
            var pagedResult = new PagedResult<UserViewModels>()
            {
                TotalRecords = totalRow,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = data
            };
            return new ApiSuccessResult<PagedResult<UserViewModels>>(pagedResult);
        }

        public async Task<ApiResult<UserViewModels>> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return new ApiErrorResult<UserViewModels>("User không tồn tại");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var userVm = new UserViewModels()
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive,
                Roles = roles
            };
            return new ApiSuccessResult<UserViewModels>(userVm);
        }
        public async Task<ApiResult<bool>> RoleAssign(Guid id, RoleAssignRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return new ApiErrorResult<bool>("Tài khoản không tồn tại");
            }
            var removedRoles = request.Roles.Where(x => x.Selected == false).Select(x => x.Name).ToList();
            foreach (var roleName in removedRoles)
            {
                if (await _userManager.IsInRoleAsync(user, roleName) == true)
                {
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                }
            }
            await _userManager.RemoveFromRolesAsync(user, removedRoles);

            var addedRoles = request.Roles.Where(x => x.Selected).Select(x => x.Name).ToList();
            foreach (var roleName in addedRoles)
            {
                if (await _userManager.IsInRoleAsync(user, roleName) == false)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }

            return new ApiSuccessResult<bool>();
        }

        public async Task<ApiResult<bool>> Update(Guid id, UserUpdateRequest request)
        {
            if(await _userManager.Users.AnyAsync(x=>x.Email==request.Email && x.Id != id.ToString()))
            {
                return new ApiErrorResult<bool>("Email đã tồn tại");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.DateOfBirth = request.DateOfBirth;
            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = request.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return new ApiSuccessResult<bool>();
            }
            return new ApiErrorResult<bool>("Cập nhật không thành công");
        }

        public async Task<ApiResult<bool>> Delete(Guid id, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return new ApiErrorResult<bool>("User không tồn tại");
            }
            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return new ApiSuccessResult<bool>();
            }
            return new ApiErrorResult<bool>("Không thành công");
        }
        public async Task<ApiResult<bool>> ForgotPassword(string email)
        {
            try
            {
                // Tìm user bằng email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new ApiErrorResult<bool>("Email không tồn tại trong hệ thống");
                }

                // Kiểm tra trạng thái tài khoản
                if (!user.IsActive)
                {
                    return new ApiErrorResult<bool>("Tài khoản đã bị vô hiệu hóa");
                }

                // Tạo mật khẩu mới ngẫu nhiên
                var newPassword = GenerateSecurePassword();

                // Tạo token reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Reset password
                var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!resetResult.Succeeded)
                {
                    return new ApiErrorResult<bool>("Lỗi đặt lại mật khẩu: " + string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }

                // Chuẩn bị nội dung email
                var emailSubject = "Mật khẩu mới cho tài khoản GoFood của bạn";
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Xin chào {user.FirstName} {user.LastName},</h2>
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                        <p>Mật khẩu mới của bạn là: <strong style='background-color: #f8f9fa; padding: 5px 10px; border-radius: 4px;'>{newPassword}</strong></p>
                        <p style='color: #dc3545;'><strong>Lưu ý:</strong> Vui lòng copy chính xác mật khẩu, kể cả các ký tự đặc biệt.</p>
                        <p>Vì lý do bảo mật, vui lòng đăng nhập và đổi mật khẩu ngay sau khi nhận được email này.</p>
                        <p style='color: #7f8c8d;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng liên hệ với chúng tôi ngay.</p>
                        <p>Trân trọng,<br>GoFood Team</p>
                    </div>";

                // Gửi email
                await _emailSender.SendEmailAsync(user.Email, emailSubject, emailBody);

                return new ApiSuccessResult<bool>
                {
                    IsSuccessed = true,
                    Message = "Mật khẩu mới đã được gửi đến email của bạn",
                    ResultObj = true
                };
            }
            catch (Exception ex)
            {
                return new ApiErrorResult<bool>($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private string GenerateSecurePassword()
        {
            // Đảm bảo mật khẩu đáp ứng yêu cầu của Identity
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "@#$%^&*";
            
            var random = new Random();
            var chars = new char[12];

            // Đảm bảo có ít nhất một ký tự từ mỗi nhóm bắt buộc
            chars[0] = upperCase[random.Next(upperCase.Length)];     // Chữ hoa
            chars[1] = lowerCase[random.Next(lowerCase.Length)];     // Chữ thường
            chars[2] = digits[random.Next(digits.Length)];           // Số
            chars[3] = specialChars[random.Next(specialChars.Length)]; // Ký tự đặc biệt

            // Điền các ký tự còn lại
            string allChars = upperCase + lowerCase + digits + specialChars;
            for (int i = 4; i < chars.Length; i++)
            {
                chars[i] = allChars[random.Next(allChars.Length)];
            }

            // Xáo trộn mảng để tạo mật khẩu ngẫu nhiên
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = chars[i];
                chars[i] = chars[j];
                chars[j] = temp;
            }

            return new string(chars);
        }

        public async Task<ApiResult<bool>> ChangePassword(Guid id, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return new ApiErrorResult<bool>("Người dùng không tồn tại");
            }

            // Kiểm tra mật khẩu hiện tại
            var isValidPassword = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isValidPassword)
            {
                return new ApiErrorResult<bool>("Mật khẩu hiện tại không đúng");
            }

            // Đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<bool>("Đổi mật khẩu không thành công: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return new ApiSuccessResult<bool>
            {
                IsSuccessed = true,
                Message = "Đổi mật khẩu thành công",
                ResultObj = true
            };
        }
    }
}
