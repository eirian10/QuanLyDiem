using QuanLyDiem.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }
        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
        [Required]
        public int CourseClassId { get; set; }
        [ForeignKey("CourseClassId")]
        public CourseClass? CourseClass { get; set; }

        [Range(0.0, 10.0, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public double? ProcessScore { get; set; } // Điểm quá trình

        [Range(0.0, 10.0, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public double? FinalScore { get; set; } // Điểm cuối kỳ


    }
}