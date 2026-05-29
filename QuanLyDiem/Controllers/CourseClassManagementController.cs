using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;

namespace QuanLyDiem.Controllers
{
    [Route("CourseClassManagement")]
    public class CourseClassManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseClassManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /CourseClassManagement
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var courseClasses = await _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Include(c => c.Lecturer)
                .OrderBy(c => c.ClassCode)
                .ToListAsync();

            return View(courseClasses);
        }

        // GET: /CourseClassManagement/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // GET: /CourseClassManagement/CreateModal
        [HttpGet("CreateModal")]
        public IActionResult CreateModal()
        {
            LoadDropdowns();
            return PartialView("_CourseClassForm", new CourseClass());
        }

        // POST: /CourseClassManagement/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseClass courseClass)
        {
            if (await IsClassCodeDuplicated(courseClass.ClassCode))
            {
                ModelState.AddModelError(nameof(courseClass.ClassCode), "Mã lớp học phần đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.CourseClasses.Add(courseClass);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(courseClass.SubjectId, courseClass.SemesterId, courseClass.LecturerId);
            return View(courseClass);
        }

        // GET: /CourseClassManagement/Edit/1
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var courseClass = await _context.CourseClasses.FindAsync(id);

            if (courseClass == null)
            {
                return NotFound();
            }

            LoadDropdowns(courseClass.SubjectId, courseClass.SemesterId, courseClass.LecturerId);
            return View(courseClass);
        }

        // GET: /CourseClassManagement/EditModal/1
        [HttpGet("EditModal/{id:int}")]
        public async Task<IActionResult> EditModal(int id)
        {
            var courseClass = await _context.CourseClasses.FindAsync(id);

            if (courseClass == null)
            {
                return NotFound();
            }

            LoadDropdowns(courseClass.SubjectId, courseClass.SemesterId, courseClass.LecturerId);
            return PartialView("_CourseClassForm", courseClass);
        }

        // POST: /CourseClassManagement/Edit/1
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseClass courseClass)
        {
            if (id != courseClass.CourseClassId)
            {
                return NotFound();
            }

            if (await IsClassCodeDuplicated(courseClass.ClassCode, courseClass.CourseClassId))
            {
                ModelState.AddModelError(nameof(courseClass.ClassCode), "Mã lớp học phần đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.CourseClasses.Update(courseClass);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CourseClassExists(courseClass.CourseClassId))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            LoadDropdowns(courseClass.SubjectId, courseClass.SemesterId, courseClass.LecturerId);
            return View(courseClass);
        }

        // GET: /CourseClassManagement/Delete/1
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var courseClass = await _context.CourseClasses
                .Include(c => c.Semester)
                .Include(c => c.Subject)
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.CourseClassId == id);

            if (courseClass == null)
            {
                return NotFound();
            }

            return View(courseClass);
        }

        // POST: /CourseClassManagement/Delete/1
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseClass = await _context.CourseClasses.FindAsync(id);

            if (courseClass == null)
            {
                return NotFound();
            }

            _context.CourseClasses.Remove(courseClass);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? selectedSubjectId = null, int? selectedSemesterId = null, int? selectedLecturerId = null)
        {
            ViewData["SubjectId"] = new SelectList(
                _context.Subjects.OrderBy(s => s.SubjectName),
                "SubjectId",
                "SubjectName",
                selectedSubjectId
            );

            ViewData["SemesterId"] = new SelectList(
                _context.Semesters
                    .OrderBy(s => s.AcademicYear)
                    .ThenBy(s => s.Term)
                    .Select(s => new
                    {
                        s.SemesterId,
                        DisplayName = s.Term + " - " + s.AcademicYear
                    }),
                "SemesterId",
                "DisplayName",
                selectedSemesterId
            );

            ViewData["LecturerId"] = new SelectList(
                _context.Users
                    .Where(u => u.Role == "Lecturer")
                    .OrderBy(u => u.FullName),
                "UserId",
                "FullName",
                selectedLecturerId
            );
        }

        private async Task<bool> IsClassCodeDuplicated(string classCode, int? currentCourseClassId = null)
        {
            return await _context.CourseClasses.AnyAsync(c =>
                c.ClassCode == classCode &&
                (!currentCourseClassId.HasValue || c.CourseClassId != currentCourseClassId.Value)
            );
        }

        private async Task<bool> CourseClassExists(int id)
        {
            return await _context.CourseClasses.AnyAsync(c => c.CourseClassId == id);
        }
    }
}