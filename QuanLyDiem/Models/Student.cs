using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDiem.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mã sinh viên phải bao gồm chính xác 10 chữ")]
        public string StudentCode { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Lớp sinh hoạt không được để trống")]
        [MaxLength(50)]
        public string HomeroomClass { get; set; } // Lớp sinh hoạt (VD: CNTT K44)

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Required(ErrorMessage = "Email là bắt buộc")]
        public string Email { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; } //update
    }
}