using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GoFood.ViewModels.Catalog.Combo
{
    public class ComboCreateRequest
    {
        [Required(ErrorMessage = "Tên combo không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        public IFormFile Image { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Combo phải có ít nhất một sản phẩm")]
        public List<Guid> ProductIds { get; set; }

        [Required(ErrorMessage = "Số lượng sản phẩm không được để trống")]
        public List<int> Quantities { get; set; }
    }
} 