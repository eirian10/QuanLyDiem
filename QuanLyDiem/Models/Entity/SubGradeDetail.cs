using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class SubGradeDetail
    {
        [Key]
        public int SubGradeID { get; set; }
        public string SectionClassID { get; set; } = null!;
        public string StudentID { get; set; } = null!;
        public string GradeCategory { get; set; } = null!; // 'Midterm' hoặc 'Final'
        public string? AssessmentMethod { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal Score { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal Weight { get; set; }
        public string? AssessmentNote { get; set; }

        [ForeignKey("SectionClassID")]
        public virtual SectionClass SectionClass { get; set; } = null!;
        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; } = null!;
    }
}
