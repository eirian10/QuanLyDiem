using System.Diagnostics.CodeAnalysis;

namespace QuanLyDiem.Models
{
    public class StudentGpaDTO
    {
        public required string StudentCode { get; set; }
        public required string FullName { get; set; }

        public double? FinalScore10 { get; set; } // Điểm tổng kết hệ 10 (ví dụ: 7.5)
        public double? GpaSystem4 { get; set; }    // Điểm GPA hệ 4 (ví dụ: 3.0)
        public string LetterGrade { get; set; } = "-"; // Điểm chữ (A, B, C, D, F)
    }
}
