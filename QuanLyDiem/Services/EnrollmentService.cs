using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuanLyDiem.Data;
using QuanLyDiem.Models;

namespace QuanLyDiem.Services
{
    public class EnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseClass>> GetLecturerClassesAsync(int lecturerId, int? semesterId = null)
        {
            var query = _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Where(c => c.LecturerId == lecturerId)
                .AsQueryable();

            if (semesterId.HasValue)
                query = query.Where(c => c.SemesterId == semesterId.Value);

            return await query.ToListAsync();
        }

        public async Task<List<Semester>> GetAllSemestersAsync()
        {
            return await _context.Semesters
                .OrderByDescending(s => s.AcademicYear)
                .ThenBy(s => s.Term)
                .ToListAsync();
        }

        public async Task<List<CourseClass>> GetAllClassesAsync(int? semesterId = null)
        {
            var query = _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .AsQueryable();

            if (semesterId.HasValue)
                query = query.Where(c => c.SemesterId == semesterId.Value);

            return await query.ToListAsync();
        }

        public async Task<CourseClass?> GetClassDetailsAsync(int classId)
        {
            return await _context.CourseClasses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.CourseClassId == classId);
        }

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

        public async Task<(bool IsSuccess, string Message)> AddStudentManualAsync(int courseClassId, string studentCode)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentCode == studentCode);
            if (student == null)
                return (false, "Không tìm thấy sinh viên với mã vừa nhập.");

            var currentClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.CourseClassId == courseClassId);
            if (currentClass == null)
                return (false, "Lớp học phần không tồn tại trên hệ thống.");

            // Check này đã bao quát cả trường hợp trùng đúng lớp hiện tại
            bool isAlreadyInSubjectThisSemester = await _context.Enrollments
                .AnyAsync(e => e.StudentId == student.StudentId
                            && e.CourseClass.SubjectId == currentClass.SubjectId
                            && e.CourseClass.SemesterId == currentClass.SemesterId);

            if (isAlreadyInSubjectThisSemester)
                return (false, "Sinh viên này đã đăng ký môn học này trong học kỳ hiện tại.");

            _context.Enrollments.Add(new Enrollment
            {
                CourseClassId = courseClassId,
                StudentId = student.StudentId
            });
            await _context.SaveChangesAsync();

            return (true, "Thêm sinh viên vào lớp thành công!");
        }

        public async Task<(bool IsSuccess, string Message)> ImportExcelAsync(int courseClassId, IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return (false, "Vui lòng chọn file Excel hợp lệ.");

            var currentClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.CourseClassId == courseClassId);
            if (currentClass == null)
                return (false, "Lớp học phần không tồn tại trên hệ thống.");

            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Eirian");

            // Load trước toàn bộ StudentId đã enroll môn này trong học kỳ này
            var enrolledStudentIds = (await _context.Enrollments
                .Where(e => e.CourseClass.SubjectId == currentClass.SubjectId
                         && e.CourseClass.SemesterId == currentClass.SemesterId)
                .Select(e => e.StudentId)
                .ToListAsync())
                .ToHashSet();

            var allStudents = (await _context.Students.ToListAsync())
                .ToDictionary(s => s.StudentCode, s => s);

            int countAdded = 0;
            var errorMessages = new List<string>();
            var toAdd = new List<Enrollment>();

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var studentCodeObj = worksheet.Cells[row, 1].Value;
                if (studentCodeObj == null) continue;

                string studentCode = studentCodeObj.ToString()!.Trim();

                if (!allStudents.TryGetValue(studentCode, out var student))
                {
                    errorMessages.Add($"Dòng {row}: Mã SV '{studentCode}' không tồn tại trong hệ thống.");
                    continue;
                }

                // Check trùng với DB (đã enroll môn này học kỳ này)
                if (enrolledStudentIds.Contains(student.StudentId))
                {
                    errorMessages.Add($"Dòng {row}: Sinh viên '{studentCode}' đã đăng ký môn này trong học kỳ hiện tại.");
                    continue;
                }

                // Check trùng trong chính file Excel
                if (toAdd.Any(e => e.StudentId == student.StudentId))
                {
                    errorMessages.Add($"Dòng {row}: Mã SV '{studentCode}' bị trùng trong file Excel.");
                    continue;
                }

                toAdd.Add(new Enrollment
                {
                    CourseClassId = courseClassId,
                    StudentId = student.StudentId
                });
                countAdded++;
            }

            if (toAdd.Count > 0)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Enrollments.AddRange(toAdd);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Lỗi khi lưu dữ liệu: {ex.Message}");
                }
            }

            string tongKet = $"Đã thêm thành công {countAdded} sinh viên.";

            if (errorMessages.Any())
                return (true, tongKet + "<br/>Chi tiết các dòng bỏ qua:<br/>" + string.Join("<br/>", errorMessages));

            return (true, tongKet);
        }

        public async Task<(bool IsSuccess, string Message)> RemoveStudentAsync(int courseClassId, int studentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseClassId == courseClassId && e.StudentId == studentId);

            if (enrollment == null)
                return (false, "Không tìm thấy thông tin đăng ký lớp của sinh viên này.");

            // Enrollment có ProcessScore và FinalScore — xóa là mất điểm luôn, cần chặn
            if (enrollment.ProcessScore.HasValue || enrollment.FinalScore.HasValue)
                return (false, "Không thể xóa: sinh viên này đã được nhập điểm. Vui lòng xóa điểm trước.");

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return (true, "Đã xóa sinh viên khỏi lớp học phần thành công.");
        }
    }
}