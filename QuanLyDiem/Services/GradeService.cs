using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDiem.Services
{
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;

        public GradeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách sinh viên và điểm số để đổ lên Form Nhập Điểm (Giữ nguyên phong cách của bạn)
        public async Task<List<ScoreEntryDTO>> GetScoresByCourseClassIdAsync(int courseClassId)
        {
            return await (from e in _context.Enrollments
                          join s in _context.Students on e.StudentId equals s.StudentId
                          where e.CourseClassId == courseClassId
                          select new ScoreEntryDTO
                          {
                              EnrollmentId = e.EnrollmentId,
                              StudentCode = s.StudentCode,
                              FullName = $"{s.LastName} {s.FirstName}", // Đã tối ưu bằng cú pháp mới trực quan
                              ProcessScore = e.ProcessScore,
                              FinalScore = e.FinalScore
                          }).ToListAsync();
        }

        // 2. Lưu điểm hàng loạt từ danh sách DTO gửi về (Chỉ cập nhật những dòng thực sự thay đổi)
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

        // 3. Lấy dữ liệu báo cáo GPA - Đã tối ưu hiệu năng SQL theo cách của bạn
        public async Task<List<StudentGpaDTO>> GetClassGpaReportAsync(int courseClassId)
        {
            // Bước 3.1: Dùng cú pháp Query tối ưu để chỉ SELECT những cột cần và tính toán số điểm số
            var gpaList = await (from e in _context.Enrollments
                                 join s in _context.Students on e.StudentId equals s.StudentId                           
                                 join cc in _context.CourseClasses on e.CourseClassId equals cc.CourseClassId
                                 join sub in _context.Subjects on cc.SubjectId equals sub.SubjectId
                                 where e.CourseClassId == courseClassId
                                 select new StudentGpaDTO
                                 {
                                     StudentCode = s.StudentCode,
                                     FullName = $"{s.LastName} {s.FirstName}",

                                     // Tính điểm hệ 10 và làm tròn đến 1 chữ số thập phân
                                     FinalScore10 = Math.Round(
                                         ((e.ProcessScore ?? 0) * sub.ProcessWeight) +
                                         ((e.FinalScore ?? 0) * sub.FinalWeight), 1),

                                     // Tính trực tiếp hệ 4 từ hệ 10 và làm tròn đến 1 chữ số thập phân
                                     GpaSystem4 = Math.Round(
                                         ((((e.ProcessScore ?? 0) * sub.ProcessWeight) +
                                           ((e.FinalScore ?? 0) * sub.FinalWeight)) * 4.0) / 10.0, 1),

                                     LetterGrade = "-" // Để mặc định để xử lý ở bước sau
                                 }).ToListAsync();

            // Bước 3.2: Duyệt nhanh trong bộ nhớ RAM để gán Điểm Chữ (Tránh tạo SQL cồng kềnh)
            foreach (var item in gpaList)
            {
                item.LetterGrade = CalculateLetterGrade(item.FinalScore10);
            }

            return gpaList;
        }

        // 4. Hàm phân tích thống kê 3 loại dữ liệu điểm để tạo dữ liệu cho 3 biểu đồ biệt lập
        public Dictionary<string, int> GetChartStatistics(List<StudentGpaDTO> gpaList)
        {
            // Khởi tạo sẵn giá trị bằng 0 cho tất cả các đầu điểm
            var letterCounts = new Dictionary<string, int>
            {
                { "A+", 0 }, { "A", 0 }, { "B+", 0 }, { "B", 0 }, { "C", 0 }, { "D", 0 }, { "F", 0 }
            };

            if (gpaList != null)
            {
                foreach (var item in gpaList)
                {
                    // Kiểm tra chắc chắn ký tự điểm chữ tồn tại trong Dictionary trước khi cộng dồn
                    if (!string.IsNullOrEmpty(item.LetterGrade) && letterCounts.ContainsKey(item.LetterGrade))
                    {
                        letterCounts[item.LetterGrade]++;
                    }
                }
            }

            return letterCounts;
        }

        // --- Hàm Helper gán điểm chữ dựa trên mốc điểm hệ 10 ---
        private string CalculateLetterGrade(double? final10)
        {
            if (!final10.HasValue) return "Chưa xét";
            
            if (final10 >= 9.0) return "A+"; // Từ 9.0 đến 10.0
            if (final10 >= 8.0) return "A";  // Từ 8.0 đến 8.9
            if (final10 >= 7.0) return "B+"; // Từ 7.0 đến 7.9
            if (final10 >= 6.0) return "B";  // Từ 6.0 đến 6.9
            if (final10 >= 5.0) return "C";  // Từ 5.0 đến 5.9
            if (final10 >= 4.0) return "D";  // Từ 4.0 đến 4.9

            return "F"; // Từ 0 đến 3.9 (Học lại)
        }
    }
}