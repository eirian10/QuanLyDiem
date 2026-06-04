using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;

namespace QuanLyDiem.Services
{
    public class CourseClassManagementService
    {
        private readonly ApplicationDbContext _context;
        public CourseClassManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseClass>> GetAllAsync()
        {
            return await _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Include(c => c.Lecturer)
                .OrderBy(c => c.ClassCode)
                .ToListAsync();
        }

        public async Task<CourseClass?> GetByIdAsync(int id)
        {
            return await _context.CourseClasses.FindAsync(id);
        }

        public async Task<CourseClass?> GetDetailsAsync(int id)
        {
            return await _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.CourseClassId == id);
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> CreateAsync(CourseClass courseClass)
        {
            var validationMessage = await ValidateCourseClassAsync(courseClass);

            if (validationMessage != null)
            {
                return (false, validationMessage);
            }

            _context.CourseClasses.Add(courseClass);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateAsync(int id, CourseClass courseClass)
        {
            if (id != courseClass.CourseClassId)
            {
                return (false, "Lớp học phần không hợp lệ.");
            }

            var oldCourseClass = await _context.CourseClasses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourseClassId == id);

            if (oldCourseClass == null)
            {
                return (false, "Không tìm thấy lớp học phần.");
            }

            if (oldCourseClass.SubjectId != courseClass.SubjectId)
            {
                var hasScore = await HasScoreAsync(id);

                if (hasScore)
                {
                    return (false, "Không thể đổi môn học vì lớp học phần đã phát sinh dữ liệu điểm.");
                }
            }

            var validationMessage = await ValidateCourseClassAsync(courseClass, courseClass.CourseClassId);

            if (validationMessage != null)
            {
                return (false, validationMessage);
            }

            _context.CourseClasses.Update(courseClass);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteAsync(int id)
        {
            var courseClass = await _context.CourseClasses.FindAsync(id);

            if (courseClass == null)
            {
                return (false, "Không tìm thấy lớp học phần.");
            }

            var hasEnrollment = await HasEnrollmentAsync(id);

            if (hasEnrollment)
            {
                return (false, "Không thể xóa lớp học phần vì đã có sinh viên đăng ký.");
            }

            _context.CourseClasses.Remove(courseClass);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public SelectList GetSubjectSelectList(int? selectedSubjectId = null)
        {
            var subjects = _context.Subjects
                .OrderBy(s => s.SubjectName)
                .ToList();

            return new SelectList(subjects, "SubjectId", "SubjectName", selectedSubjectId);
        }

        public SelectList GetSemesterSelectList(int? selectedSemesterId = null)
        {
            var semesters = _context.Semesters
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.Term)
                .Select(s => new
                {
                    s.SemesterId,
                    DisplayName = s.Term + " - " + s.AcademicYear
                })
                .ToList();

            return new SelectList(semesters, "SemesterId", "DisplayName", selectedSemesterId);
        }

        public SelectList GetLecturerSelectList(int? selectedLecturerId = null)
        {
            var lecturers = _context.Users
                .Where(u => u.Role == "Lecturer")
                .OrderBy(u => u.FullName)
                .ToList();

            return new SelectList(lecturers, "UserId", "FullName", selectedLecturerId);
        }

        private async Task<string?> ValidateCourseClassAsync(CourseClass courseClass, int? currentCourseClassId = null)
        {
            if (string.IsNullOrWhiteSpace(courseClass.ClassCode))
            {
                return "Mã lớp học phần không được để trống.";
            }

            if (courseClass.SemesterId <= 0)
            {
                return "Vui lòng chọn học kỳ.";
            }

            if (courseClass.SubjectId <= 0)
            {
                return "Vui lòng chọn môn học.";
            }

            if (!courseClass.LecturerId.HasValue || courseClass.LecturerId.Value <= 0)
            {
                return "Vui lòng chọn giảng viên phụ trách.";
            }

            var isDuplicated = await _context.CourseClasses.AnyAsync(c =>
                c.ClassCode == courseClass.ClassCode &&
                (!currentCourseClassId.HasValue || c.CourseClassId != currentCourseClassId.Value)
            );

            if (isDuplicated)
            {
                return "Mã lớp học phần đã tồn tại.";
            }

            return null;
        }

        private async Task<bool> HasEnrollmentAsync(int courseClassId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.CourseClassId == courseClassId);
        }

        private async Task<bool> HasScoreAsync(int courseClassId)
        {
            return await _context.Enrollments
                .AnyAsync(e =>
                    e.CourseClassId == courseClassId &&
                    (e.ProcessScore != null || e.FinalScore != null)
                );
        }
    }
}