using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;

namespace QuanLyDiem.Services
{
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;

        public GradeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách sinh viên và điểm số để đổ lên Form
        public async Task<List<ScoreEntryDTO>> GetScoresByCourseClassIdAsync(int courseClassId)
        {
            return await (from e in _context.Enrollments
                         join s in _context.Students on e.StudentId equals s.StudentId
                         where e.CourseClassId == courseClassId
                         select new ScoreEntryDTO
                         {
                             EnrollmentId = e.EnrollmentId,
                             StudentCode = s.StudentCode,
                             FullName = string.Concat(s.LastName, " ", s.FirstName),
                             ProcessScore = e.ProcessScore,
                             FinalScore = e.FinalScore
                         }).ToListAsync();
        }

        // 2. Lưu điểm hàng loạt từ danh sách DTO gửi về
        public async Task<bool> UpdateScoresAsync(List<ScoreEntryDTO> scores)
        {
            if (scores == null || !scores.Any()) return false;

            // 2.1. Lấy danh sách ID để truy vấn dữ liệu hiện tại từ DB
            var enrollmentIds = scores.Select(s => s.EnrollmentId).ToList();
            var enrollments = await _context.Enrollments
                .Where(e => enrollmentIds.Contains(e.EnrollmentId))
                .ToListAsync();

            bool hasChanges = false;

            // 2.2. Duyệt qua danh sách điểm gửi lên
            foreach (var scoreDto in scores)
            {
                var entry = enrollments.FirstOrDefault(e => e.EnrollmentId == scoreDto.EnrollmentId);

                if (entry != null)
                {
                    // Kiểm tra xem giá trị gửi lên có khác với giá trị hiện tại trong DB không
                    bool isProcessChanged = entry.ProcessScore != scoreDto.ProcessScore;
                    bool isFinalChanged = entry.FinalScore != scoreDto.FinalScore;

                    if (isProcessChanged || isFinalChanged)
                    {
                        entry.ProcessScore = scoreDto.ProcessScore;
                        entry.FinalScore = scoreDto.FinalScore;
                        hasChanges = true; // Đánh dấu là có sự thay đổi
                    }
                }
            }

            // 2.3. Nếu không có gì thay đổi thì không cần gọi SaveChanges
            if (!hasChanges) return true;

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
