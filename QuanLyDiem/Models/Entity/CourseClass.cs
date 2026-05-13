using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models.Entity
{
    public class CourseClass
    {
        [Key]
        public int CourseClassId { get; set; }

        [Required(ErrorMessage = "Mã lớp học phần không được để trống")]
        [MaxLength(50)]
        public int ClassCode { get; set; } // VD: IT01_N01

        [Required(ErrorMessage = "Học kỳ không được để trống")]
        [MaxLength(20)]
        public string Semester { get; set; } // VD: HK1_2025_2026

        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        public int? LecturerId { get; set; }
        [ForeignKey("LecturerId")]
        public User Lecturer { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}