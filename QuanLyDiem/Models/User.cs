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
        public string Username { get; set; } = string.Empty; // Gán chuỗi rỗng mặc định để C# không cảnh báo lỗi Nullable gạch đỏ

        [ValidateNever] // Ra lệnh cho hệ thống KHÔNG kiểm tra dữ liệu ô này khi nhận form (Vì mật khẩu được tạo tự động bằng code ở Controller)
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")] 
        [MaxLength(100)] 
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn Khoa công tác.")]
        public int FacultyId { get; set; } 

        [ForeignKey("FacultyId")] // Khai báo mối quan hệ: Liên kết thuộc tính FacultyId ở trên với đối tượng Faculty ở dưới
        public Faculty? Faculty { get; set; } // Thuộc tính điều hướng (Navigation Property) giúp gọi nhanh tên Khoa bằng lệnh: user.Faculty.FacultyName

        [ValidateNever] // Bỏ qua xác thực mặc định của framework trên giao diện vì trường này được gán tự động "Lecturer" ở Controller
        [Required] 
        [RegularExpression("^(Admin|Lecturer)$", ErrorMessage = "Quyền phải là Admin hoặc Lecturer.")] // Chỉ chấp nhận chuỗi nhập vào là "Admin" hoặc "Lecturer", nhập khác sẽ báo lỗi
        public string Role { get; set; } = string.Empty;
        public ICollection<CourseClass>? CourseClasses { get; set; } = new List<CourseClass>();
    }
}