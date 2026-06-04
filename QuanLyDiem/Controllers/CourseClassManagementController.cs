using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDiem.Models;
using QuanLyDiem.Services;

namespace QuanLyDiem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("CourseClassManagement")]
    public class CourseClassManagementController : Controller
    {
        private readonly CourseClassManagementService _courseClassService;

        public CourseClassManagementController(CourseClassManagementService courseClassService)
        {
            _courseClassService = courseClassService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var courseClasses = await _courseClassService.GetAllAsync();
            return View(courseClasses);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseClass courseClass)
        {
            if (ModelState.IsValid)
            {
                var result = await _courseClassService.CreateAsync(courseClass);

                if (result.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }

                AddCourseClassErrorToModelState(result.ErrorMessage);
            }

            LoadDropdowns(courseClass.SubjectId, courseClass.SemesterId, courseClass.LecturerId);
            return View(courseClass);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var courseClass = await _courseClassService.GetByIdAsync(id);

            if (courseClass == null)
            {
                return NotFound();
            }

            LoadDropdowns(courseClass.SubjectId, courseClass.SemesterId, courseClass.LecturerId);
            return View(courseClass);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseClass courseClass)
        {
            if (ModelState.IsValid)
            {
                var result = await _courseClassService.UpdateAsync(id, courseClass);

                if (result.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }

                AddCourseClassErrorToModelState(result.ErrorMessage);
            }

            LoadDropdowns(courseClass.SubjectId, courseClass.SemesterId, courseClass.LecturerId);
            return View(courseClass);
        }

        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _courseClassService.DeleteAsync(id);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Xóa lớp học phần thành công.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Không thể xóa lớp học phần.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? selectedSubjectId = null, int? selectedSemesterId = null, int? selectedLecturerId = null)
        {
            ViewData["SubjectId"] = _courseClassService.GetSubjectSelectList(selectedSubjectId);
            ViewData["SemesterId"] = _courseClassService.GetSemesterSelectList(selectedSemesterId);
            ViewData["LecturerId"] = _courseClassService.GetLecturerSelectList(selectedLecturerId);
        }

        private void AddCourseClassErrorToModelState(string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return;
            }

            var lowerError = errorMessage.ToLower();

            if (lowerError.Contains("mã lớp"))
            {
                ModelState.AddModelError(nameof(CourseClass.ClassCode), errorMessage);
            }
            else if (lowerError.Contains("học kỳ"))
            {
                ModelState.AddModelError(nameof(CourseClass.SemesterId), errorMessage);
            }
            else if (lowerError.Contains("môn học") || lowerError.Contains("đổi môn học"))
            {
                ModelState.AddModelError(nameof(CourseClass.SubjectId), errorMessage);
            }
            else if (lowerError.Contains("giảng viên"))
            {
                ModelState.AddModelError(nameof(CourseClass.LecturerId), errorMessage);
            }
            else
            {
                ModelState.AddModelError("", errorMessage);
            }
        }
    }
}