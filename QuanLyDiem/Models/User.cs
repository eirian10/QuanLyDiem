using System.Collections.Generic; // Thư viện dùng để làm việc với các kiểu danh sách tập hợp (như ICollection, List)
using System.ComponentModel.DataAnnotations; // Thư viện chứa các thuộc tính ràng buộc dữ liệu (Validation) như [Key], [Required]...
using System.ComponentModel.DataAnnotations.Schema; // Thư viện chứa các quy tắc thiết lập cấu trúc bảng database như [ForeignKey]
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thư viện chứa thuộc tính [ValidateNever] để kiểm soát việc check form

namespace QuanLyDiem.Models
{
    // Lớp User đại diện cho cấu trúc bảng "Users" nằm trong Cơ sở dữ liệu của bạn
    public class User
    {
        [Key] // Đánh dấu thuộc tính liền sau nó là Khóa chính (Primary Key) của bảng dưới Database
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")] // Bắt buộc phải nhập từ giao diện, nếu bỏ trống sẽ hiện ErrorMessage tiếng Việt
        [StringLength(50, ErrorMessage = "Tài khoản không quá 50 ký tự.")] // Giới hạn độ dài tối đa của chuỗi khi lưu trữ là 50 ký tự
        public string Username { get; set; } = string.Empty; // Gán chuỗi rỗng mặc định để C# không cảnh báo lỗi Nullable gạch đỏ

        [ValidateNever] // Ra lệnh cho hệ thống KHÔNG kiểm tra dữ liệu ô này khi nhận form (Vì mật khẩu được tạo tự động bằng code ở Controller)
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")] // Bắt buộc người dùng điền họ và tên
        [MaxLength(100)] // Quy định độ dài tối đa của chuỗi họ tên trong SQL là NVARCHAR(100)
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")] // Hệ thống tự động bắt lỗi nếu user nhập thiếu dấu @ hoặc sai cú pháp email
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn Khoa công tác.")]
        public int FacultyId { get; set; } // Khóa ngoại (Foreign Key) dùng để lưu ID của Khoa liên kết với Giảng viên này

        [ForeignKey("FacultyId")] // Khai báo mối quan hệ: Liên kết thuộc tính FacultyId ở trên với đối tượng Faculty ở dưới
        public Faculty? Faculty { get; set; } // Thuộc tính điều hướng (Navigation Property) giúp bạn gọi nhanh tên Khoa bằng lệnh: user.Faculty.FacultyName

        [ValidateNever] // Bỏ qua xác thực mặc định của framework trên giao diện vì trường này được gán tự động "Lecturer" ở Controller
        [Required] // Ép buộc trường dữ liệu này dưới Database luôn phải có giá trị (NOT NULL)
        [RegularExpression("^(Admin|Lecturer)$", ErrorMessage = "Quyền phải là Admin hoặc Lecturer.")] // Chỉ chấp nhận chuỗi nhập vào là "Admin" hoặc "Lecturer", nhập khác sẽ báo lỗi
        public string Role { get; set; } = string.Empty;

        // Thể hiện mối quan hệ 1 - Nhiều (Một Giảng viên có thể phụ trách giảng dạy nhiều Lớp học phần)
        // Dấu ? nghĩa là danh sách này có thể null/rỗng khi vừa khởi tạo tài khoản mới mà chưa được phân công lớp
        public ICollection<CourseClass>? CourseClasses { get; set; } = new List<CourseClass>();
    }
}