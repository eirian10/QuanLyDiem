using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models.Entity
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [StringLength(20)]
        public string StudentCode { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; }

        public DateTime DateOfBirth { get; set; }

        [MaxLength(50)]
        public string HomeroomClass { get; set; } // Lớp sinh hoạt (VD: CNTT K44)

        [EmailAddress]
        public string Email { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}