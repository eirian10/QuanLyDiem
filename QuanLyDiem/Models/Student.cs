using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDiem.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mã sinh viên phải bao gồm chính xác 10 chữ")]
        public string StudentCode { get; set; }

        [Required(ErrorMessage = "Họ và tên lót không được để trống.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Tên sinh viên không được để trống.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp sinh hoạt.")]
        public int HomeroomClassId { get; set; }

        [ForeignKey("HomeroomClassId")]
        public HomeroomClass? HomeroomClass { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Required(ErrorMessage = "Email là bắt buộc")]
        public string Email { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();
    }
}