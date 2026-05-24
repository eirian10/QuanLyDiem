using QuanLyDiem.Models;

namespace QuanLyDiem.ViewModels
{
    public class GradebookViewModel
    {
        public int CourseClassId { get; set; }
        public string ClassCode { get; set; } // Hiển thị trên tiêu đề trang
        public string SubjectName { get; set; }

        // Danh sách các dòng nhập điểm
        public List<ScoreEntryDTO> Students { get; set; } = new List<ScoreEntryDTO>();
    }
}
