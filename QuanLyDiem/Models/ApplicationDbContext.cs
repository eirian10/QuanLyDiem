using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Models; // Sửa MyProject thành tên namespace của bạn

namespace QuanLyDiem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<CourseClass> CourseClasses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Các ràng buộc duy nhất (Unique Constraints) để tránh nhập trùng dữ liệu
            builder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            builder.Entity<Student>().HasIndex(s => s.StudentCode).IsUnique();
            builder.Entity<Subject>().HasIndex(s => s.SubjectCode).IsUnique();
        }
    }
}