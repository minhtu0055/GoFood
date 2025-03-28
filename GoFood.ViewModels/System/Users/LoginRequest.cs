using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.ViewModels.System.Users
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(100)]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MaxLength(100)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
