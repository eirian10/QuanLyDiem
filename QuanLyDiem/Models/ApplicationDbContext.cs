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

        public DbSet<Faculty> Faculties { get; set; } = null!;
        public DbSet<Semester> Semesters { get; set; } = null!;
        public DbSet<HomeroomClass> HomeroomClasses { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;

        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<CourseClass> CourseClasses { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Các ràng buộc duy nhất (Unique Constraints) để tránh nhập trùng dữ liệu
            builder.Entity<Faculty>().HasIndex(f => f.FacultyCode).IsUnique();

            builder.Entity<Semester>().HasIndex(s => new { s.Term, s.AcademicYear }).IsUnique();
            builder.Entity<Enrollment>().HasIndex(e => new { e.StudentId, e.CourseClassId }).IsUnique();
            builder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            builder.Entity<Student>().HasIndex(s => s.StudentCode).IsUnique();
            builder.Entity<Subject>().HasIndex(s => s.SubjectCode).IsUnique();
            builder.Entity<HomeroomClass>().HasIndex(c => c.ClassName).IsUnique();
        }
    }
}