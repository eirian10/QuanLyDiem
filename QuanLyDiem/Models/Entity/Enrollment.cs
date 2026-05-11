using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models.Entity
{
    public class Enrollment
    {
        public string SectionClassID { get; set; } = null!;
        public string StudentID { get; set; } = null!;
        public string? EnrollmentType { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal Grade10 { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal Grade4 { get; set; }
        public string? LetterGrade { get; set; }

        [ForeignKey("SectionClassID")]
        public virtual SectionClass SectionClass { get; set; } = null!;
        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; } = null!;
    }
}
