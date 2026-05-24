using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models
{
    public class ScoreEntryDTO
    {
        [Required]
        public int EnrollmentId { get; set; }
        public string StudentCode { get; set; }
        public string FullName { get; set; }
        [Range(0, 10, ErrorMessage = "Điểm quá trình phải nằm trong khoảng từ 0 đến 10")]
        public double? ProcessScore { get; set; }
        [Range(0, 10, ErrorMessage = "Điểm cuối kỳ phải nằm trong khoảng từ 0 đến 10")]
        public double? FinalScore { get; set; }
    }
}
