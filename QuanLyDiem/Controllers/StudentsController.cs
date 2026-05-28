using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyDiem.Services;
using System.Threading.Tasks;
using QuanLyDiem.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyDiem.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class StudentsController : Controller
    {
        private readonly StudentService _studentService;

        public StudentsController(StudentService studentService)
        {
            _studentService = studentService;
        }

        // Thay đổi tham số homeroomClass sang kiểu int? homeroomClassId để đồng bộ bộ lọc
        public async Task<IActionResult> Index(int? homeroomClassId, string? searchString)
        {
            var classes = await _studentService.GetUniqueClassesAsync();

            // Khởi tạo SelectList: "HomeroomClassId" là Value xử lý, "ClassName" là Text hiển thị
            ViewBag.Classes = new SelectList(classes, "HomeroomClassId", "ClassName", homeroomClassId);
            ViewBag.SelectedClass = homeroomClassId;
            ViewBag.SearchString = searchString;

            var students = await _studentService.GetAllStudentsAsync(homeroomClassId, searchString);
            return View(students);
        }

        public async Task<IActionResult> Create()
        {
            var classes = await _studentService.GetUniqueClassesAsync();
            ViewBag.HomeroomClassId = new SelectList(classes, "HomeroomClassId", "ClassName");
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

            var classes = await _studentService.GetUniqueClassesAsync();
            ViewBag.HomeroomClassId = new SelectList(classes, "HomeroomClassId", "ClassName", student.HomeroomClassId);
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

        // BỔ SUNG: Hàm lấy dữ liệu cũ đưa lên form Edit công khai
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var classes = await _studentService.GetUniqueClassesAsync();
            ViewBag.HomeroomClassId = new SelectList(classes, "HomeroomClassId", "ClassName", student.HomeroomClassId);
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
                    ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại hoặc không thể cập nhật.");
                }
            }

            var classes = await _studentService.GetUniqueClassesAsync();
            ViewBag.HomeroomClassId = new SelectList(classes, "HomeroomClassId", "ClassName", std.HomeroomClassId);
            return View(std);
        }

        // SỬA LỖI CHÍNH TẢ: Thay đổi Detete thành Delete chuẩn chỉnh
        public async Task<IActionResult> Delete(int id)
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