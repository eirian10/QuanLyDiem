using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Models.Entity;

namespace QuanLyDiem.Data
{
    public class QuanLyDiemDbContext : DbContext
    {
        // Constructor dùng cho Dependency Injection trong Program.cs
        public QuanLyDiemDbContext(DbContextOptions<QuanLyDiemDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng dữ liệu
        public DbSet<User> Users { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<SectionClass> SectionClasses { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<SubGradeDetail> SubGradeDetails { get; set; }
        public DbSet<ClassGradeSummary> ClassGradeSummaries { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Username (Email) là duy nhất
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 2. Mối quan hệ 1-1 giữa User và Lecturer
            modelBuilder.Entity<Lecturer>()
                .HasOne(l => l.User)
                .WithOne(u => u.Lecturer)
                .HasForeignKey<Lecturer>(l => l.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Khóa chính hỗn hợp cho ClassGradeSummary
            modelBuilder.Entity<ClassGradeSummary>()
                .HasKey(c => new { c.SectionClassID, c.StudentID });

            // 4. Khóa chính hỗn hợp cho Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasKey(e => new { e.SectionClassID, e.StudentID });

            // 5. Cấu hình làm tròn điểm số (decimal 4,2)
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(4, 2)");
            }
        }
    }
}