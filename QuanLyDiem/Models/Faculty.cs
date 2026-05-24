using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models
{
    public class Faculty
    {
        [Key]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Mã khoa không được để trống.")]
        public string FacultyCode { get; set; } = null!; // Ví dụ: 'CNTT', 'TOAN'

        [Required(ErrorMessage = "Tên khoa không được để trống.")]
        public string FacultyName { get; set; } = null!; // Ví dụ: 'Công nghệ thông tin'

        // Một khoa quản lý nhiều lớp sinh hoạt và nhiều giảng viên
        public ICollection<HomeroomClass>? HomeroomClasses { get; set; } = new List<HomeroomClass>();
        public ICollection<User>? Users { get; set; } = new List<User>();
    }
}
