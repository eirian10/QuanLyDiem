using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models
{
    public class Semester
    {
        [Key]
        public int SemesterId { get; set; }

        [Required(ErrorMessage = "Học kỳ không được để trống.")]
        [RegularExpression("^(HK1|HK2|HK3)$", ErrorMessage = "Học kỳ chỉ có thể là HK1, HK2 hoặc HK3.")]
        public string Term { get; set; } = null!; // HK1, HK2, HK3

        [Required(ErrorMessage = "Năm học không được để trống.")]
        public string AcademicYear { get; set; } = null!; // Ví dụ: '2025-2026'

        // Quan hệ 1 - Nhiều
        public ICollection<CourseClass>? CourseClasses { get; set; } = new List<CourseClass>();
    }
}

