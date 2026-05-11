using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Username { get; set; } = null!; // Email sẽ được lưu ở đây

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = null!; // 'Admin' hoặc 'Lecturer'

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liên kết 1-1 với Profile giáo viên
        public virtual Lecturer? Lecturer { get; set; }
    }
}
