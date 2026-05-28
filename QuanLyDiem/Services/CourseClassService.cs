using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuanLyDiem.Data;
using QuanLyDiem.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDiem.Services
{
    public class CourseClassService
    {
        private readonly ApplicationDbContext _context;

        public CourseClassService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách lớp học phần của giảng viên cụ thể
        public async Task<List<CourseClass>> GetLecturerClassesAsync(int lecturerId)
        {
            return await _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Where(c => c.LecturerId == lecturerId)
                .ToListAsync();
        }

        // Lấy thông tin chung của lớp học phần
        public async Task<CourseClass?> GetClassDetailsAsync(int classId)
        {
            return await _context.CourseClasses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.CourseClassId == classId);
        }

        // Lấy danh sách học viên của lớp: Sắp xếp theo StudentCode (Tăng dần), Họ tên, Ngày sinh, Lớp sinh hoạt
        public async Task<List<Student>> GetClassStudentsAsync(int classId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.HomeroomClass)
                .Where(e => e.CourseClassId == classId)
                .OrderBy(e => e.Student.StudentCode) // Sắp xếp theo mã sinh viên tăng dần
                .Select(e => e.Student)
                .ToListAsync();
        }

        // Thêm sinh viên bằng tay qua Mã sinh viên
        public async Task<(bool IsSuccess, string Message)> AddStudentManualAsync(int courseClassId, string studentCode)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
            if (student == null)
            {
                return (false, "Không tìm thấy sinh viên với mã vừa nhập.");
            }

            bool isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseClassId == courseClassId && e.StudentId == student.StudentId);

            if (isEnrolled)
            {
                return (false, "Sinh viên này đã có sẵn trong lớp học phần.");
            }

            var enrollment = new Enrollment { CourseClassId = courseClassId, StudentId = student.StudentId };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return (true, "Thêm sinh viên vào lớp thành công!");
        }

        // Import sinh viên hàng loạt từ file Excel
        public async Task<(bool IsSuccess, string Message)> ImportExcelAsync(int courseClassId, IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return (false, "Vui lòng chọn file Excel hợp lệ.");
            }

            // 1. KIỂM TRA KHÓA NGOẠI LỚP HỌC PHẦN TRƯỚC KHI ĐỌC FILE
            bool classExists = await _context.CourseClasses.AnyAsync(c => c.CourseClassId == courseClassId);
            if (!classExists)
            {
                return (false, $"Lỗi: Không tìm thấy lớp học phần với ID {courseClassId} trong hệ thống.");
            }
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Eirian");
            // Đã sửa lỗi bôi đỏ: Sử dụng đường dẫn namespace đầy đủ cho EPPlus mới nhất
            int countAdded = 0;
            int countSkipped = 0;

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension?.Rows ?? 0;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var studentCodeObj = worksheet.Cells[row, 1].Value;
                        if (studentCodeObj == null) continue;

                        string studentCode = studentCodeObj.ToString().Trim();

                        // Tìm sinh viên dựa vào Mã sinh viên trong DB trường
                        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
                        if (student != null)
                        {
                            // Kiểm tra sinh viên đã có trong lớp học phần này chưa
                            bool isEnrolled = await _context.Enrollments
                                .AnyAsync(e => e.CourseClassId == courseClassId && e.StudentId == student.StudentId);

                            if (!isEnrolled)
                            {
                                _context.Enrollments.Add(new Enrollment
                                {
                                    CourseClassId = courseClassId,
                                    StudentId = student.StudentId
                                });
                                countAdded++;
                            }
                            else
                            {
                                countSkipped++;
                            }
                        }
                        else
                        {
                            countSkipped++; // Bỏ qua nếu mã sinh viên không tồn tại trong hệ thống
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return (true, $"Đã thêm thành công {countAdded} sinh viên từ file Excel. (Bỏ qua {countSkipped} sinh viên trùng hoặc không tồn tại)");
        }

        // Xóa sinh viên khỏi lớp học phần
        public async Task<(bool IsSuccess, string Message)> RemoveStudentAsync(int courseClassId, int studentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseClassId == courseClassId && e.StudentId == studentId);

            if (enrollment == null)
            {
                return (false, "Không tìm thấy thông tin đăng ký lớp của sinh viên này.");
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return (true, "Đã xóa sinh viên khỏi lớp học phần thành công.");
        }
    }
}