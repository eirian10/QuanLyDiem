using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models.Entity
{
    public class ClassGradeSummary
    {
        public string SectionClassID { get; set; } = null!;
        public string StudentID { get; set; } = null!;

        [Column(TypeName = "decimal(4,2)")]
        public decimal AttendanceScore { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal MidtermScore { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal FinalScore { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal AttendanceWeight { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal MidtermWeight { get; set; }

        [ForeignKey("SectionClassID")]
        public virtual SectionClass SectionClass { get; set; } = null!;
        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; } = null!;
    }
}
