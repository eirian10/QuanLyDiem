using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDiem.Services;
using System.Threading.Tasks;

namespace QuanLyDiem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")] 
    public class EnrollmentsController : Controller
    {
        private readonly EnrollmentService _enrollmentService;

        public EnrollmentsController(EnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        public async Task<IActionResult> Index(int? semesterId, string? search)
        {
            ViewBag.SelectedSemesterId = semesterId;
            ViewBag.Search = search;

            ViewBag.Semesters = await _enrollmentService.GetAllSemestersAsync();

            if (User.IsInRole("Admin"))
            {
                var allDbClasses = await _enrollmentService.GetAllClassesAsync(semesterId,search);
                return View(allDbClasses);
            }
            //Tim trong Claims xem có claim nào tên là "UserId" 
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int lecturerId)) //Chuyen sang int xong gan cho leacturer id
            {
                return Challenge();
            }

            var lecturerClasses = await _enrollmentService.GetLecturerClassesAsync(lecturerId, semesterId, search);
            return View(lecturerClasses);
        }

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

        [HttpPost]
        public async Task<IActionResult> AddStudentManual(int courseClassId, string studentCode)
        {
            var result = await _enrollmentService.AddStudentManualAsync(courseClassId, studentCode);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;
            //dinh kem courseClassId de khi redirect ve details thi van con id de lay duoc danh sach sinh vien
            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(int courseClassId, IFormFile excelFile)
        {
            var result = await _enrollmentService.ImportExcelAsync(courseClassId, excelFile);
            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = courseClassId });
        }

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