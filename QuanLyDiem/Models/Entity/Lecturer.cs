using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models.Entity
{
    public class Lecturer
    {
        [Key]
        public string LecturerID { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        public string? Department { get; set; }

        [EmailAddress]
        public string? Email { get; set; } // Thông tin liên lạc hiển thị trên hồ sơ

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<SectionClass> SectionClasses { get; set; } = new List<SectionClass>();
    }
}
