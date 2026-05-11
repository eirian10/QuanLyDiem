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

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public int CourseClassId { get; set; }
        [ForeignKey("CourseClassId")]
        public CourseClass CourseClass { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public double? ProcessScore { get; set; } // Điểm quá trình

        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public double? FinalScore { get; set; } // Điểm cuối kỳ

        // ==========================================
        // CÁC THUỘC TÍNH TỰ TÍNH TOÁN (Không lưu DB)
        // ==========================================

        [NotMapped]
        public double? Total10 =>
            (ProcessScore.HasValue && FinalScore.HasValue && CourseClass?.Subject != null)
            ? Math.Round((double)(ProcessScore * CourseClass.Subject.ProcessWeight +
                                  FinalScore * CourseClass.Subject.FinalWeight), 1)
            : null;

        [NotMapped]
        public string LetterGrade
        {
            get
            {
                if (!Total10.HasValue) return null;
                var score = Total10.Value;

                if (score >= 9.0) return "A+";
                if (score >= 8.0) return "A";
                if (score >= 7.0) return "B+";
                if (score >= 6.0) return "B";
                if (score >= 5.0) return "C";
                if (score >= 4.0) return "D";
                return "F";
            }
        }

        [NotMapped]
        public double? Grade4
        {
            get
            {
                return LetterGrade switch
                {
                    "A+" => 4.0,
                    "A" => 3.5,
                    "B+" => 3.0,
                    "B" => 2.5,
                    "C" => 2.0,
                    "D" => 1.5,
                    "F" => 0.0,
                    _ => null
                };
            }
        }
    }
}