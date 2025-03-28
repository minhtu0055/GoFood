using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GoFood.ViewModels.Catalog.Products
{
    public class ProductCreateRequest
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng sản phẩm không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Danh mục không được để trống")]
        public Guid CategoryId { get; set; }

        public List<IFormFile> Images { get; set; }
    }
}
