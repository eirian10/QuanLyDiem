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
        } //constructor của controller

        // GET: /Subjects
        // GET: /Subjects/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index() //hàm để hiển thị danh sách môn học
        {
            var subjects = await _subjectService.GetAllAsync(); //gọi service lấy toàn bộ danh sách môn học
            return View(subjects);//trả về view
        }

        // GET: /Subjects/Create
        [HttpGet("Create")]
        public IActionResult Create()//hàm để hiển thị giao diện thêm môn học
        {
            return View();
        }

       

        // POST: /Subjects/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken] //chống giả mạo request
        public async Task<IActionResult> Create(Subject subject) //hàm này nhận môn học từ form gửi lên
        {
            if (ModelState.IsValid) //nếu hợp lệ
            {
                var result = await _subjectService.CreateAsync(subject);//gọi service để thực hiện thêm môn học mới

                if (result.IsSuccess)//nếu thêm môn học thành công
                {
                    return RedirectToAction(nameof(Index));//chuyển người dùng về trang danh sách môn học
                }

                AddSubjectErrorToModelState(result.ErrorMessage);//nếu thêm hoặc sửa môn học thất bại thì báo lỗi
            }

            return View(subject);//trả về view
        }

        // GET: /Subjects/Edit/1
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id) //khai báo hàm edit
        {
            var subject = await _subjectService.GetByIdAsync(id); //gọi service để tìm môn học theo id

            if (subject == null)//không tìm thấy môn học
            {
                return NotFound();//báo lỗi
            }

            return View(subject);//tìm thấy môn học thì trả về view
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