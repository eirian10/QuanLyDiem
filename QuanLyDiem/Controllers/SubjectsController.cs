using Microsoft.AspNetCore.Mvc;
using QuanLyDiem.Models;
using QuanLyDiem.Services;

namespace QuanLyDiem.Controllers
{
    [Route("Subjects")]
    public class SubjectsController : Controller
    {
        private readonly SubjectService _subjectService;

        public SubjectsController(SubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        // GET: /Subjects
        // GET: /Subjects/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var subjects = await _subjectService.GetAllAsync();
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
            if (ModelState.IsValid)
            {
                var result = await _subjectService.CreateAsync(subject);

                if (result.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }

                AddSubjectErrorToModelState(result.ErrorMessage);
            }

            return View(subject);
        }

        // GET: /Subjects/Edit/1
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _subjectService.GetByIdAsync(id);

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
            var subject = await _subjectService.GetByIdAsync(id);

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
            if (ModelState.IsValid)
            {
                var result = await _subjectService.UpdateAsync(id, subject);

                if (result.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }

                AddSubjectErrorToModelState(result.ErrorMessage);
            }

            return View(subject);
        }

        // GET: /Subjects/Delete/1
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _subjectService.GetByIdAsync(id);

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
            var isDeleted = await _subjectService.DeleteAsync(id);

            if (!isDeleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        private void AddSubjectErrorToModelState(string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return;
            }

            if (errorMessage.Contains("Mã môn học"))
            {
                ModelState.AddModelError(nameof(Subject.SubjectCode), errorMessage);
                return;
            }

            ModelState.AddModelError("", errorMessage);
        }
    }

}