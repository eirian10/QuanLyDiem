using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDiem.Services
{
    public class StudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Student> Data, int TotalRecords)> GetAllStudentsAsync(
            int? homeroomClassId, string? searchString, int pageNumber, int pageSize)
        {
            var query = _context.Students.Include(s => s.HomeroomClass).AsQueryable();

            if (homeroomClassId > 0)
            {
                query = query.Where(s => s.HomeroomClassId == homeroomClassId);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.ToLower().Trim();

                query = query.Where(s => s.StudentCode.Contains(keyword)
                                      || s.FirstName.ToLower().Contains(keyword)
                                      || s.LastName.ToLower().Contains(keyword)
                                      || (s.LastName.ToLower() + " " + s.FirstName.ToLower()).Contains(keyword));
            }

            //  Đếm tổng số sinh viên thỏa mãn bộ lọc
            int totalRecords = await query.CountAsync();

            //Lay ra danh sach sinh vien ap dung phan trang 
            var data = await query
                .OrderBy(s => s.StudentCode)
                .Skip((pageNumber - 1) * pageSize) //So dong can bo qua
                .Take(pageSize) //Lay so dong can hieu thi sau khi da bo qua 
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<List<HomeroomClass>> GetUniqueClassesAsync()
        {
            return await _context.HomeroomClasses.ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.HomeroomClass)
                .FirstOrDefaultAsync(s => s.StudentId == id);
        }

        public async Task<bool> AddStudentAsync(Student student)
        {
            var exists = await _context.Students
                .AnyAsync(s => s.StudentCode == student.StudentCode);

            if (exists)
            {
                return false;
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            var existingStudent = await _context.Students.FindAsync(student.StudentId);
            if (existingStudent == null)
            {
                return false;
            }

            // Kiểm tra trùng mã sinh viên với người khác khi sửa
            var codeExists = await _context.Students
                .AnyAsync(s => s.StudentCode == student.StudentCode && s.StudentId != student.StudentId);
            if (codeExists)
            {
                return false;
            }

            // Cập nhật chuẩn theo các thuộc tính mới của Model Student
            existingStudent.StudentCode = student.StudentCode;
            existingStudent.LastName = student.LastName;
            existingStudent.FirstName = student.FirstName;
            existingStudent.Gender = student.Gender;
            existingStudent.DateOfBirth = student.DateOfBirth;
            existingStudent.HomeroomClassId = student.HomeroomClassId;
            existingStudent.Email = student.Email;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return false;
            }
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}