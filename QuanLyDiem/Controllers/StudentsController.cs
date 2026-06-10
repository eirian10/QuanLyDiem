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
        public async Task<IActionResult> Index(int? homeroomClassId, string? searchString, int pageNumber = 1)
        {
            int pageSize = 10; // Đặt cố định hiển thị so sinh viên mỗi trang

            var classes = await _studentService.GetUniqueClassesAsync();
            ViewBag.Classes = new SelectList(classes, "HomeroomClassId", "ClassName", homeroomClassId);
            ViewBag.SelectedClass = homeroomClassId; //Giup ko bi mat chu khi loc
            ViewBag.SearchString = searchString;

            // Lấy ra danh sách sinh viên 
            var result = await _studentService.GetAllStudentsAsync(homeroomClassId, searchString, pageNumber, pageSize);

            // Tính toán và lưu dữ liệu phân trang vào ViewBag để View sử dụng đúng định dạng mong muốn
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalRecords = result.TotalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalRecords / pageSize);

            return View(result.Data);
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
                    ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại ");
                }
            }

            var classes = await _studentService.GetUniqueClassesAsync();
            ViewBag.HomeroomClassId = new SelectList(classes, "HomeroomClassId", "ClassName", student.HomeroomClassId);
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