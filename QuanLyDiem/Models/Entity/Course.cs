using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class Course
    {
        [Key]
        public string CourseID { get; set; } = null!;
        [Required]
        public string CourseName { get; set; } = null!;
        public int Credits { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal ProcessWeight { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal FinalWeight { get; set; }

        public virtual ICollection<SectionClass> SectionClasses { get; set; } = new List<SectionClass>();
    }
}
