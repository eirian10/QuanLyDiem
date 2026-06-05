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

        /// <summary>
        /// Lấy danh sách lớp học phần của một giảng viên - Có hỗ trợ lọc theo Học kỳ
        /// </summary>
        public async Task<List<CourseClass>> GetLecturerClassesAsync(int lecturerId, int? semesterId = null)
        {
            var query = _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Where(c => c.LecturerId == lecturerId).AsQueryable();

            // Nếu truyền semesterId thì tiến hành lọc theo học kỳ đó
            if (semesterId.HasValue)
            {
                query = query.Where(c => c.SemesterId == semesterId.Value);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Dành cho admin: Lấy tất cả lớp học phần trên hệ thống - Có hỗ trợ lọc theo Học kỳ
        /// </summary>
        ///         // Lấy danh sách tất cả các lớp sinh hoạt để nạp vào Dropdown tương tác trên View
        public async Task<List<Semester>> GetAllSemestersAsync()
        {
            return await _context.Semesters
                .OrderByDescending(s => s.AcademicYear) // Năm học mới nhất xếp lên đầu
                .ThenBy(s => s.Term)                  // Sắp xếp theo HK1, HK2, HK3
                .ToListAsync();
        }
        public async Task<List<CourseClass>> GetAllClassesAsync(int? semesterId = null)
        {
            var query = _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject).AsQueryable();

            // Nếu truyền semesterId thì tiến hành lọc theo học kỳ đó
            if (semesterId.HasValue)
            {
                query = query.Where(c => c.SemesterId == semesterId.Value);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một lớp học phần
        /// </summary>
        public async Task<CourseClass?> GetClassDetailsAsync(int classId)
        {
            return await _context.CourseClasses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.CourseClassId == classId);
        }

        /// <summary>
        /// Lấy danh sách sinh viên trong lớp học phần (Sắp xếp tăng dần theo Mã sinh viên)
        /// </summary>
        public async Task<List<Student>> GetClassStudentsAsync(int classId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.HomeroomClass)
                .Where(e => e.CourseClassId == classId)
                .OrderBy(e => e.Student.StudentCode)
                .Select(e => e.Student)
                .ToListAsync();
        }

        /// <summary>
        /// Thêm sinh viên thủ công bằng Mã sinh viên (Đã sửa logic: Cho phép học lại/cải thiện ở học kỳ khác)
        /// </summary>
        public async Task<(bool IsSuccess, string Message)> AddStudentManualAsync(int courseClassId, string studentCode)
        {
            // 1. Kiểm tra sinh viên có tồn tại không
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
            if (student == null)
            {
                return (false, "Không tìm thấy sinh viên với mã vừa nhập.");
            }

            // 2. Lấy thông tin của lớp học phần hiện tại để lấy SubjectId và SemesterId
            var currentClass = await _context.CourseClasses.FirstOrDefaultAsync(c => c.CourseClassId == courseClassId);
            if (currentClass == null)
            {
                return (false, "Lớp học phần không tồn tại trên hệ thống.");
            }

            // 3. Logic mới: Chỉ chặn nếu trùng môn trong CÙNG MỘT HỌC KỲ (Hỗ trợ học lại / cải thiện ở học kỳ khác)
            bool isAlreadyInSubjectThisSemester = await _context.Enrollments
                .AnyAsync(e => e.StudentId == student.StudentId
                            && e.CourseClass.SubjectId == currentClass.SubjectId
                            && e.CourseClass.SemesterId == currentClass.SemesterId);

            if (isAlreadyInSubjectThisSemester)
            {
                return (false, "Sinh viên này đã đăng ký lớp học khác cho môn học này trong cùng học kỳ.");
            }

            // 4. Hợp lệ thì tiến hành lưu vào DB
            var enrollment = new Enrollment { CourseClassId = courseClassId, StudentId = student.StudentId };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return (true, "Thêm sinh viên vào lớp thành công!");
        }

        /// <summary>
        /// Import danh sách sinh viên từ file Excel (Đã sửa logic: Cho phép học lại/cải thiện ở học kỳ khác)
        /// </summary>
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

                        // 2. Logic mới: Kiểm tra trùng môn trong CÙNG MỘT HỌC KỲ
                        bool isAlreadyInSubjectThisSemester = await _context.Enrollments
                            .AnyAsync(e => e.StudentId == student.StudentId
                                        && e.CourseClass.SubjectId == currentClass.SubjectId
                                        && e.CourseClass.SemesterId == currentClass.SemesterId);

                        if (isAlreadyInSubjectThisSemester)
                        {
                            errorMessages.Add($"Dòng {row}: Sinh viên '{studentCode}' đã học môn này ở lớp khác trong cùng học kỳ.");
                            continue;
                        }

                        // 3. Nếu hợp lệ thì thêm vào database
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
            string tongKet = $"Đã thêm thành công {countAdded} sinh viên.";

            if (errorMessages.Any())
            {
                string chiTietLoi = "<br/> Chi tiết các dòng bỏ qua:<br/>" + string.Join("<br/>", errorMessages);
                return (true, tongKet + chiTietLoi);
            }

            return (true, tongKet);
        }

        /// <summary>
        /// Xóa sinh viên khỏi lớp học phần
        /// </summary>
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