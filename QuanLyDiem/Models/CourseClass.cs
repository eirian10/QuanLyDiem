using QuanLyDiem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models
{
    public class CourseClass
    {
        [Key]
        public int CourseClassId { get; set; }
        [Required(ErrorMessage = "Mã lớp học phần không được để trống.")]
        [MaxLength(50, ErrorMessage = "Mã lớp học phần không được vượt quá 50 ký tự.")]
        public string ClassCode { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn học kỳ.")]
        public int SemesterId { get; set; }

        [ForeignKey("SemesterId")]
        public Semester? Semester { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giảng viên phụ trách.")]
        public int? LecturerId { get; set; }

        [ForeignKey("LecturerId")]
        public User? Lecturer { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();
    }
}