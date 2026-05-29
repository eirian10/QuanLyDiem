using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;

namespace QuanLyDiem.Controllers
{
    [Route("Subjects")]
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Subjects
        // GET: /Subjects/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var subjects = await _context.Subjects
                .OrderBy(s => s.SubjectCode)
                .ToListAsync();

            return View(subjects);
        }

        // GET: /Subjects/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // GET: /Subjects/CreateModal
        [HttpGet("CreateModal")]
        public IActionResult CreateModal()
        {
            return PartialView("_SubjectForm", new Subject());
        }

        // POST: /Subjects/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject)
        {
            ValidateSubjectWeights(subject);

            if (await IsSubjectCodeDuplicated(subject.SubjectCode))
            {
                ModelState.AddModelError(nameof(subject.SubjectCode), "Mã môn học đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(subject);
        }

        // GET: /Subjects/Edit/1
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // GET: /Subjects/EditModal/1
        [HttpGet("EditModal/{id:int}")]
        public async Task<IActionResult> EditModal(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            return PartialView("_SubjectForm", subject);
        }

        // POST: /Subjects/Edit/1
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.SubjectId)
            {
                return NotFound();
            }

            ValidateSubjectWeights(subject);

            if (await IsSubjectCodeDuplicated(subject.SubjectCode, subject.SubjectId))
            {
                ModelState.AddModelError(nameof(subject.SubjectCode), "Mã môn học đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Subjects.Update(subject);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await SubjectExists(subject.SubjectId))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            return View(subject);
        }

        // GET: /Subjects/Delete/1
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.SubjectId == id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // POST: /Subjects/Delete/1
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void ValidateSubjectWeights(Subject subject)
        {
            const double epsilon = 0.0001;

            if (Math.Abs((subject.ProcessWeight + subject.FinalWeight) - 1.0) > epsilon)
            {
                ModelState.AddModelError(
                    "",
                    "Tổng trọng số điểm quá trình và điểm cuối kỳ phải bằng 1. Ví dụ: 0.4 + 0.6 = 1."
                );
            }
        }

        private async Task<bool> IsSubjectCodeDuplicated(string subjectCode, int? currentSubjectId = null)
        {
            return await _context.Subjects.AnyAsync(s =>
                s.SubjectCode == subjectCode &&
                (!currentSubjectId.HasValue || s.SubjectId != currentSubjectId.Value)
            );
        }

        private async Task<bool> SubjectExists(int id)
        {
            return await _context.Subjects.AnyAsync(s => s.SubjectId == id);
        }
    }
}