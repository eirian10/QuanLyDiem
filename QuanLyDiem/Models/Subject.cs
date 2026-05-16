using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Mã môn học không được để trống")]
        [StringLength(20)]
        public string SubjectCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên môn học không được để trống")]
        [MaxLength(100)]
        public string SubjectName { get; set; } = null!;

        [Required(ErrorMessage = "Số tín chỉ không được để trống.")]
        [Range(1, 6, ErrorMessage = "Số tín chỉ phải từ 1 đến 6.")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Trọng số điểm quá trình không được để trống.")]
        [Range(0.0, 1.0, ErrorMessage = "Trọng số phải nằm trong khoảng từ 0.0 đến 1.0.")]
        public double ProcessWeight { get; set; }

        [Required(ErrorMessage = "Trọng số điểm cuối kỳ không được để trống.")]
        [Range(0.0, 1.0, ErrorMessage = "Trọng số phải nằm trong khoảng từ 0.0 đến 1.0.")]
        public double FinalWeight { get; set; }

        public ICollection<CourseClass>? CourseClasses { get; set; }
    }
}