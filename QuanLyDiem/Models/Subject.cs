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
        public string SubjectCode { get; set; }

        [Required(ErrorMessage = "Tên môn học không được để trống")]
        [MaxLength(100)]
        public string SubjectName { get; set; }

        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        public int Credits { get; set; }

        [Required]
        public double ProcessWeight { get; set; } // Trọng số quá trình (VD: 0.3)

        [Required]
        public double FinalWeight { get; set; } // Trọng số thi (VD: 0.7)

        public ICollection<CourseClass> CourseClasses { get; set; }
    }
}