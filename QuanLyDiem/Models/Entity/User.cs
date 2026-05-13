using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string Department { get; set; } // Khoa/Bộ môn (Admin có thể để trống)

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // Chỉ lưu "Admin" hoặc "Lecturer"

        // Một giảng viên có thể phụ trách nhiều lớp
        public ICollection<CourseClass> CourseClasses { get; set; }
    
}
}