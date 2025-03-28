using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.ViewModels.Common
{
    public class ApiErrorResult<T> : ApiResult<T>
    {
        public string[] ValidationErrors { get; set; } //Một mảng chứa các lỗi validation (nếu có )
        public ApiErrorResult() { }
        public ApiErrorResult(string message) // Khi api gặp lỗi nó có thể gọi contructor này để đặt 
        {
            IsSuccessed = false; // Báo api thất bại
            Message = message; // Lưu thông báo lỗi
        }
        public ApiErrorResult(string[] validationErrors) 
        {
            IsSuccessed = false; // Báo api thất bại
            ValidationErrors = validationErrors;
        }
    }
}
