using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Services;
using QuanLyDiem.ViewModels;

namespace QuanLyDiem.Controllers
{
    public class GradeController : Controller
    {
        private readonly IGradeService _gradeService;
        private readonly ApplicationDbContext _context;

        public GradeController(IGradeService gradeService, ApplicationDbContext context)
        {
            _gradeService = gradeService;
            _context = context;
        }
        [Route("Grade/EnterScores/{id}")]
        public async Task<IActionResult> EnterScores(int id)
        {
            var courseClass = await _context.CourseClasses
                .Include(cc => cc.Subject)
                .FirstOrDefaultAsync(cc => cc.CourseClassId == id);

            if (courseClass == null) return NotFound();

            var viewModel = new GradebookViewModel
            {
                CourseClassId = id,
                ClassCode = courseClass.ClassCode.ToString(),
                SubjectName = courseClass.Subject.SubjectName,
                Students = await _gradeService.GetScoresByCourseClassIdAsync(id)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScores(GradebookViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _gradeService.UpdateScoresAsync(model.Students);

                // Dùng TempData để hiển thị thông báo ở trang tiếp theo
                TempData["SuccessMessage"] = "Lưu điểm thành công!";

                return RedirectToAction(nameof(EnterScores), new { id = model.CourseClassId });
            }
            return View("EnterScores", model);
        }
    }
}
