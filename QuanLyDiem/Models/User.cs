using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyDiem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, ErrorMessage = "Tài khoản không quá 50 ký tự.")]
        public string Username { get; set; }

        [ValidateNever] // Bỏ qua validate phía client/form vì controller tự sinh ngẫu nhiên
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn Khoa công tác.")]
        public int FacultyId { get; set; }

        [ForeignKey("FacultyId")]
        public Faculty? Faculty { get; set; }
        [ValidateNever]
        [Required]
        [RegularExpression("^(Admin|Lecturer)$", ErrorMessage = "Quyền phải là Admin hoặc Lecturer.")]
        public string Role { get; set; } = null!;

        public ICollection<CourseClass>? CourseClasses { get; set; } = new List<CourseClass>();
    }
}