using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class SectionClass
    {
        [Key]
        public string SectionClassID { get; set; } = null!;
        public string CourseID { get; set; } = null!;
        public string LecturerID { get; set; } = null!;
        public string? SemesterID { get; set; }
        public string? Status { get; set; } // 'Active' hoặc 'Closed'
        public int MaxCapacity { get; set; }

        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; } = null!;
        [ForeignKey("LecturerID")]
        public virtual Lecturer Lecturer { get; set; } = null!;

        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
    }
}
