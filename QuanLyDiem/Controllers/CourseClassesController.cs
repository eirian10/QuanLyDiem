using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDiem.Services;
using System.Threading.Tasks;

namespace QuanLyDiem.Controllers
{
    [Authorize(Roles = "Lecturer")] // Chỉ cho tài khoản có quyền giảng viên truy cập
    public class CourseClassesController : Controller
    {
        private readonly CourseClassService _courseClassService;

        public CourseClassesController(CourseClassService courseClassServices)
        {
            _courseClassService = courseClassServices;
        }

        // 1. Xem danh sách các lớp học phần được phân công
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int lecturerId))
            {
                return Challenge();
            }

            var classes = await _courseClassService.GetLecturerClassesAsync(lecturerId);
            return View(classes);
        }

        // 2. Xem chi tiết danh sách sinh viên trong lớp
        public async Task<IActionResult> Details(int id)
        {
            var courseClass = await _courseClassService.GetClassDetailsAsync(id);
            if (courseClass == null) return NotFound();

            var students = await _courseClassService.GetClassStudentsAsync(id);

            ViewBag.CourseClassId = id;
            ViewBag.ClassName = courseClass.ClassCode;
            ViewBag.SubjectName = courseClass.Subject?.SubjectName;

            return View(students);
        }

        // 3. Thêm thủ công bằng Mã sinh viên
        [HttpPost]
        public async Task<IActionResult> AddStudentManual(int courseClassId, string studentCode)
        {
            var result = await _courseClassService.AddStudentManualAsync(courseClassId, studentCode);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }

        // 4. Import danh sách sinh viên từ file Excel
        [HttpPost]
        public async Task<IActionResult> ImportExcel(int courseClassId, IFormFile excelFile)
        {
            var result = await _courseClassService.ImportExcelAsync(courseClassId, excelFile);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }

        // 5. Xóa sinh viên khỏi lớp học phần
        [HttpPost]
        public async Task<IActionResult> RemoveStudent(int courseClassId, int studentId)
        {
            var result = await _courseClassService.RemoveStudentAsync(courseClassId, studentId);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }
    }
}