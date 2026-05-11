using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class Student
    {
        [Key]
        public string StudentID { get; set; } = null!;
        [Required]
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? ClassID { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
