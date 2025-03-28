using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.Data.Enums
{
    public enum Status
    {
        Pending,      // Chờ xác nhận
        Confirmed,    // Đã xác nhận
        Shipping,     // Đang vận chuyển
        Completed,    // Hoàn thành
        Cancelled     // Đã hủy
    }
}
