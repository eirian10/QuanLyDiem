using Microsoft.AspNetCore.Mvc;
using QuanLyDiem.Services;
using System.Threading.Tasks;
using QuanLyDiem.Models;

namespace QuanLyDiem.Controllers
{
    public class StudentsController : Controller
    {
        private readonly StudentService _studentService;

        public StudentsController(StudentService studentService)
        {
            _studentService = studentService;
        }
        public async Task<IActionResult> Index(string homeroomClass)
        {
            ViewBag.Classes = await _studentService.GetUniqueClassesAsync();
            ViewBag.SelectedClass = homeroomClass;

            var students = await _studentService.GetAllStudentsAsync(homeroomClass);
            return View(students);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                bool isAdded = await _studentService.AddStudentAsync(student);
                if (isAdded)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại.");
                }
            }
            return View(student);
        }
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);

        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Student std)
        {
            if (id != std.StudentId) return NotFound();
            if (ModelState.IsValid)
            {
                bool isUpdated = await _studentService.UpdateStudentAsync(std);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại.");
                }
            }
            return View(std);
        }

        public async Task<IActionResult> Detete(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentService.DeleteStudentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}