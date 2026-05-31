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
    public class EnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentService(ApplicationDbContext context)
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
        //Danh cho admin 
        public async Task<List<CourseClass>> GetAllClassesAsync()
        {
            return await _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .ToListAsync(); // Không lọc Where theo LecturerId nữa!
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
            bool isAlreadyInSubject = await _context.Enrollments
                .AnyAsync(e => e.StudentId == student.StudentId && e.CourseClass.SubjectId == _context.CourseClasses.FirstOrDefault(c => c.CourseClassId == courseClassId).SubjectId);

            if (isAlreadyInSubject)
            {
                return (false, "Sinh viên này đã có sẵn trong lớp học phần.");
            }

            var enrollment = new Enrollment { CourseClassId = courseClassId, StudentId = student.StudentId };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return (true, "Thêm sinh viên vào lớp thành công!");
        }

        // Import sinh viên từ Excel - Hiển thị chi tiết từng dòng bị lỗi
        public async Task<(bool IsSuccess, string Message)> ImportExcelAsync(int courseClassId, IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return (false, "Vui lòng chọn file Excel hợp lệ.");
            }

            var currentClass = await _context.CourseClasses.FirstOrDefaultAsync(c => c.CourseClassId == courseClassId);
            if (currentClass == null)
            {
                return (false, "Lớp học phần không tồn tại trên hệ thống.");
            }

            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Eirian");
            int countAdded = 0;

            // Tạo một danh sách để chứa chi tiết các dòng bị lỗi
            var errorMessages = new List<string>();

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

                        // 1. Kiểm tra sinh viên tồn tại
                        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
                        if (student == null)
                        {
                            errorMessages.Add($"Dòng {row}: Mã SV '{studentCode}' không tồn tại trong hệ thống.");
                            continue;
                        }

                        // 2. Kiểm tra trùng theo môn học
                        bool isAlreadyInSubject = await _context.Enrollments
                            .AnyAsync(e => e.StudentId == student.StudentId && e.CourseClass.SubjectId == currentClass.SubjectId);

                        if (isAlreadyInSubject)
                        {
                            errorMessages.Add($"Dòng {row}: Sinh viên '{studentCode}' đã học môn này ở lớp khác.");
                            continue;
                        }

                        // 3. Nếu hợp lệ thì thêm vào danh sách chờ lưu
                        _context.Enrollments.Add(new Enrollment
                        {
                            CourseClassId = courseClassId,
                            StudentId = student.StudentId
                        });
                        countAdded++;
                    }

                    if (countAdded > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // --- XỬ LÝ KẾT QUẢ TRẢ VỀ ---
            string tổngKết = $"Đã thêm thành công {countAdded} sinh viên.";

            // Nếu có lỗi, nối các câu lỗi lại thành các dòng xuống hàng (<br/>) để hiển thị lên giao diện
            if (errorMessages.Any())
            {
                string chiTiếtLỗi = "<br/> Chi tiết các dòng bỏ qua:<br/>" + string.Join("<br/>", errorMessages);
                return (true, tổngKết + chiTiếtLỗi);
            }

            return (true, tổngKết);
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