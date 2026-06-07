using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;
using QuanLyDiem.Services;

[Authorize(Roles = "Admin")]
public class LecturersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    // Gộp chung vào 1 constructor duy nhất
    public LecturersController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // 1. Danh sách & Tìm kiếm
    public async Task<IActionResult> Index(string searchString)
    {
        var lecturers = _context.Users
            .Include(u => u.Faculty)
            .Where(u => u.Role == "Lecturer");

        if (!string.IsNullOrEmpty(searchString))
        {
            lecturers = lecturers.Where(u => u.FullName.Contains(searchString)
                                          || u.Email.Contains(searchString)
                                          || u.Faculty.FacultyName.Contains(searchString));
        }
        return View(await lecturers.ToListAsync());
    }

    // 2. Hiển thị form tạo mới (GET)
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
        return View();
    }

    // 3. Xử lý lưu (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        user.Role = "Lecturer";

        // 1. Kiểm tra xem Username đã tồn tại trong DB chưa
        bool isUsernameExist = await _context.Users.AnyAsync(u => u.Username == user.Username);
        if (isUsernameExist)
        {
            // Thêm lỗi vào ModelState để hiển thị ra ngoài View
            ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại trong hệ thống. Vui lòng chọn tên khác.");
        }
        // 2. BỔ SUNG: Kiểm tra xem Email đã tồn tại chưa
        bool isEmailExist = await _context.Users.AnyAsync(u => u.Email == user.Email);
        if (isEmailExist)
        {
            ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng. Vui lòng nhập email khác.");
        }

        // 2. Tạo mật khẩu ngẫu nhiên
        string rawPassword = GenerateRandomPassword();
        user.Password = new PasswordHasher<User>().HashPassword(user, rawPassword);

        if (ModelState.IsValid)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();

            // Gửi email
            await _emailService.SendEmailAsync(user.Email, "Thông tin tài khoản giảng viên",
                $"Chào bạn, tài khoản của bạn đã được tạo.\nUsername: {user.Username}\nMật khẩu: {rawPassword}");

            TempData["Success"] = "Đã thêm giảng viên và gửi thông tin qua email!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName", user.FacultyId);
        return View(user);
    }

    // 4. Xóa
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private string GenerateRandomPassword(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}