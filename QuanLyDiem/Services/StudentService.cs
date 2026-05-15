using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;
using System.Diagnostics.Eventing.Reader;
namespace QuanLyDiem.Services
{
    public class StudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        //Lay danh sach sinh vien, loc theo lop sinh hoat
        public async Task<IEnumerable<Student>> GetAllStudentsAsync(string homeroomClass, string searchString)
        {
            var query = _context.Students.AsQueryable();

            // Lọc theo lớp nếu có chọn
            if (!string.IsNullOrWhiteSpace(homeroomClass))
            {
                query = query.Where(s => s.HomeroomClass == homeroomClass);
            }

            // Lọc theo Tên hoặc Mã sinh viên nếu có nhập từ khóa
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim().ToLower();
                query = query.Where(s => s.FullName.ToLower().Contains(searchString)
                                      || s.StudentCode.ToLower().Contains(searchString));
            }

            return await query.ToListAsync();
        }
        public async Task<List<string>> GetUniqueClassesAsync()
        {
              return await _context.Students
                 .Select(s => s.HomeroomClass)
                 .Where(c => !string.IsNullOrEmpty(c))
                 .Distinct()
                 .ToListAsync();
        }
        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await _context.Students.FindAsync(id);
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
                return false; // Sinh viên không tồn tại
            }
            // Cập nhật thông tin sinh viên
            existingStudent.StudentCode = student.StudentCode;
            existingStudent.FullName = student.FullName;
            existingStudent.DateOfBirth = student.DateOfBirth;
            existingStudent.HomeroomClass = student.HomeroomClass;
            existingStudent.Email = student.Email;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return false; // Sinh viên không tồn tại
            }
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
