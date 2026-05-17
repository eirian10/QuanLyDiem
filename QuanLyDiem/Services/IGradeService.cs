using QuanLyDiem.Models;

namespace QuanLyDiem.Services
{
    public interface IGradeService
    {
        Task<List<ScoreEntryDTO>> GetScoresByCourseClassIdAsync(int courseClassId);
        Task<bool> UpdateScoresAsync(List<ScoreEntryDTO> scoreEntries);
    }
}