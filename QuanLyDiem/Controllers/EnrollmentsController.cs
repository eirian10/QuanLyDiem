using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDiem.Services;
using System.Threading.Tasks;

namespace QuanLyDiem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")] // Chỉ cho tài khoản có quyền Admin hoặc Giảng viên truy cập
    public class EnrollmentsController : Controller
    {
        private readonly EnrollmentService _enrollmentService;

        public EnrollmentsController(EnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        public async Task<IActionResult> Index(int? semesterId)
        {
            ViewBag.SelectedSemesterId = semesterId;

            // GỌI QUA SERVICE: Lấy danh sách học kỳ đẩy vào ViewBag để làm bộ lọc động
            ViewBag.Semesters = await _enrollmentService.GetAllSemestersAsync();

            // Khối xử lý phân quyền lấy danh sách lớp học phần giữ nguyên
            if (User.IsInRole("Admin"))
            {
                var allDbClasses = await _enrollmentService.GetAllClassesAsync(semesterId);
                return View(allDbClasses);
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int lecturerId))
            {
                return Challenge();
            }

            var lecturerClasses = await _enrollmentService.GetLecturerClassesAsync(lecturerId, semesterId);
            return View(lecturerClasses);
        }

        // Các hàm Details, AddStudentManual, ImportExcel, RemoveStudent bên dưới giữ nguyên...
    

        // 2. Xem chi tiết danh sách sinh viên trong lớp
        public async Task<IActionResult> Details(int id)
        {
            var courseClass = await _enrollmentService.GetClassDetailsAsync(id);
            if (courseClass == null) return NotFound();

            var students = await _enrollmentService.GetClassStudentsAsync(id);

            ViewBag.CourseClassId = id;
            ViewBag.ClassName = courseClass.ClassCode;
            ViewBag.SubjectName = courseClass.Subject?.SubjectName;

            return View(students);
        }

        // 3. Thêm thủ công bằng Mã sinh viên
        [HttpPost]
        public async Task<IActionResult> AddStudentManual(int courseClassId, string studentCode)
        {
            var result = await _enrollmentService.AddStudentManualAsync(courseClassId, studentCode);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }

        // 4. Import danh sách sinh viên từ file Excel
        [HttpPost]
        public async Task<IActionResult> ImportExcel(int courseClassId, IFormFile excelFile)
        {
            var result = await _enrollmentService.ImportExcelAsync(courseClassId, excelFile);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }

        // 5. Xóa sinh viên khỏi lớp học phần
        [HttpPost]
        public async Task<IActionResult> RemoveStudent(int courseClassId, int studentId)
        {
            var result = await _enrollmentService.RemoveStudentAsync(courseClassId, studentId);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }
    }
}