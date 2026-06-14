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

        public async Task<List<CourseClass>> GetLecturerClassesAsync(int lecturerId, int? semesterId, string? search)
        {
            var query = _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Where(c => c.LecturerId == lecturerId)
                .AsQueryable();

            if (semesterId > 0)
                query = query.Where(c => c.SemesterId == semesterId);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword =  search.ToLower().Trim();
                query = query.Where(c => c.ClassCode.ToLower().Contains(keyword)
                                       || c.Subject.SubjectName.ToLower().Contains(keyword));
            }

            return await query.ToListAsync();
        }

        public async Task<List<Semester>> GetAllSemestersAsync()
        {
            return await _context.Semesters
                .OrderByDescending(s => s.AcademicYear)
                .ThenBy(s => s.Term) //neu cung nam hoc thi xep theo hoc ky
                .ToListAsync();
        }

        public async Task<List<CourseClass>> GetAllClassesAsync(int? semesterId, string? search)
        {
            var query = _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .AsQueryable();

            if (semesterId > 0)
                query = query.Where(c => c.SemesterId == semesterId);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower().Trim();
                query = query.Where(c => c.ClassCode.ToLower().Contains(keyword)
                                       || c.Subject.SubjectName.ToLower().Contains(keyword));
            }

            return await query.ToListAsync();
        }

        public async Task<CourseClass?> GetClassDetailsAsync(int classId)
        {
            return await _context.CourseClasses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.CourseClassId == classId);
        }
        //Lấy danh sách sinh viên trong 1 lớp học phần, kèm lớp sinh hoạt, sắp xếp theo mã sinh viên
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

            // Kiểm tra xem sinh viên đã dk môn này trong học kỳ này chưa
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
          
            // Load trước toàn bộ StudentId đã enroll môn này trong học kỳ này
            var enrolledStudentIds = (await _context.Enrollments
                .Where(e => e.CourseClass.SubjectId == currentClass.SubjectId
                         && e.CourseClass.SemesterId == currentClass.SemesterId)
                .Select(e => e.StudentId) //chi lay id
                .ToListAsync())
                .ToHashSet(); //chuyển sang HashSet để tăng tốc độ tra cứu khi check trùng

            // Load toàn bộ Students 1 lần,Chuyển sang Dictionary,Key Value để tra cứu nhanh khi đọc từng dòng Excel
            var allStudents = (await _context.Students.ToListAsync())
                .ToDictionary(s => s.StudentCode, s => s);

            int countAdded = 0;
            var errorMessages = new List<string>();
            var toAdd = new List<Enrollment>(); 

            //copy file upload vào RAM, chuẩn bị cho EPPlus đọc
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            // ExcelPackage mở stream (file trong RAM)
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0; //lấy số dòng có dữ liệu trong sheet

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

                // Check đã enroll môn này học kỳ này
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
                try
                {
                    _context.Enrollments.AddRange(toAdd);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
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