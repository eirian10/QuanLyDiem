using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models
{
    public class HomeroomClass
    {
        [Key]
        public int HomeroomClassId { get; set; }

        [Required(ErrorMessage = "Tên lớp sinh hoạt không được để trống.")]
        public string ClassName { get; set; } = null!; // Ví dụ: 'CNTTK49A'

        [Required(ErrorMessage = "Vui lòng chọn Khoa quản lý.")]
        public int FacultyId { get; set; }

        [ForeignKey("FacultyId")]
        public Faculty? Faculty { get; set; } // Liên kết đến bảng Khoa

        public ICollection<Student>? Students { get; set; } = new List<Student>();
    }
}
