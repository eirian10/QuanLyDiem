using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyDiem.Models;

namespace QuanLyDiem.Services
{
    public interface IGradeService
    {
        // 1. Lấy danh sách sinh viên phục vụ form nhập điểm
        Task<List<ScoreEntryDTO>> GetScoresByCourseClassIdAsync(int courseClassId);

        // 2. Cập nhật điểm số hàng loạt xuống Database
        Task<bool> UpdateScoresAsync(List<ScoreEntryDTO> scoreEntries);

        // 3. Lấy danh sách điểm tổng kết phục vụ trang báo cáo GPA
        Task<List<StudentGpaDTO>> GetClassGpaReportAsync(int courseClassId);

        // 4. Thống kê số lượng theo điểm chữ phục vụ vẽ biểu đồ (Đã sửa kiểu dữ liệu từ List<int> thành int)
        Dictionary<string, int> GetChartStatistics(List<StudentGpaDTO> gpaList);
    }
}