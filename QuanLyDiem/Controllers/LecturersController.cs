using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;
using QuanLyDiem.Services;

// Bộ lọc bảo mật: Chỉ những tài khoản đăng nhập có quyền "Admin" mới được phép truy cập vào Controller này
[Authorize(Roles = "Admin")]
public class LecturersController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public LecturersController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // 1. TRANG DANH SÁCH & TÌM KIẾM GIẢNG VIÊN (MẶC ĐỊNH)
    public async Task<IActionResult> Index(string searchString)
    {
        // truy vấn lấy ra các User có vai trò là "Lecturer"
        // .Include(u => u.Faculty) giúp nạp kèm dữ liệu của bảng Khoa (tránh bị lỗi null khi gọi tên Khoa ở View)
        var lecturers = _context.Users
            .Include(u => u.Faculty)
            .Where(u => u.Role == "Lecturer");

        if (!string.IsNullOrEmpty(searchString))
        {
            lecturers = lecturers.Where(u => u.FullName.Contains(searchString)
                                          || u.Email.Contains(searchString)
                                          || u.Faculty!.FacultyName.Contains(searchString));
        }

        return View(await lecturers.ToListAsync());
    }

    // 2. HIỂN THỊ FORM THÊM MỚI GIẢNG VIÊN (GET)
    [HttpGet]
    public IActionResult Create()
    {
        // Bốc danh sách các Khoa từ DB lên, nạp vào ViewBag dưới dạng SelectList (gồm ID và Tên Khoa) 
        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
        return View();
    }

    // 3. XỬ LÝ LƯU GIẢNG VIÊN MỚI VÀO DATABASE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        user.Role = "Lecturer";

        bool isUsernameExist = await _context.Users.AnyAsync(u => u.Username == user.Username);
        if (isUsernameExist)
        {
            ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại trong hệ thống. Vui lòng chọn tên khác.");
        }


        bool isEmailExist = await _context.Users.AnyAsync(u => u.Email == user.Email);
        if (isEmailExist)
        {
            ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng. Vui lòng nhập email khác.");
        }

        // Tự động sinh một chuỗi mật khẩu ngẫu nhiên dài 10 ký tự bằng hàm phụ ở phía dưới
        string rawPassword = GenerateRandomPassword();

        // Tiến hành băm (mã hóa) mật khẩu thô thành chuỗi ký tự bảo mật cao trước khi lưu vào Database
        user.Password = BCrypt.Net.BCrypt.HashPassword(rawPassword);


        if (ModelState.IsValid)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();

            // Gửi email thông báo tự động chứa tài khoản và mật khẩu thô về hòm thư của giảng viên mới vừa được tạo
            await _emailService.SendEmailAsync(user.Email, "Thông tin tài khoản giảng viên",
                $"Chào bạn, tài khoản của bạn đã được tạo.\nUsername: {user.Username}\nMật khẩu: {rawPassword}");

            TempData["Success"] = "Đã thêm giảng viên và gửi thông tin qua email!";
            return RedirectToAction(nameof(Index));
        }

        // Nếu dữ liệu nhập vào form có lỗi, nạp lại menu thả xuống các Khoa và trả lại dữ liệu đang nhập lỗi về form để người dùng sửa tiếp
        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName", user.FacultyId);
        return View(user);
    }

    // 4. XỬ LÝ XÓA GIẢNG VIÊN (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa giảng viên thành công!";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa giảng viên này vì giảng viên đang có lớp học trong hệ thống!";
        }

        return RedirectToAction(nameof(Index));
    }
    // Hàm bổ trợ nội bộ: Dùng để tạo ra chuỗi ký tự ngẫu nhiên gồm chữ hoa, chữ thường, số và ký tự đặc biệt làm mật khẩu
    private string GenerateRandomPassword(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // 5. HIỂN THỊ FORM CHỈNH SỬA THÔNG TIN GIẢNG VIÊN (GET)
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var lecturer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Lecturer");
        if (lecturer == null) return NotFound();

        // Nạp danh sách các Khoa vào ViewBag để hiển thị menu Dropdown, đồng thời chọn sẵn (Select) Khoa hiện tại của giảng viên đó
        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName", lecturer.FacultyId);
        return View(lecturer);
    }

    // 6. XỬ LÝ LƯU DỮ LIỆU GIẢNG VIÊN SAU KHI SỬA (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,FullName,Email,FacultyId")] User user)
    {
        if (id != user.UserId)
        {
            return NotFound();
        }

        bool isUsernameExist = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == user.Username.ToLower() && u.UserId != id);

        if (isUsernameExist)
        {
            ModelState.AddModelError("Username", "Tên đăng nhập này đã được một giảng viên khác sử dụng!");
        }

        bool isEmailExist = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.UserId != id);

        if (isEmailExist)
        {
            ModelState.AddModelError("Email", "Địa chỉ email này đã thuộc về một giảng viên khác!");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var databaseUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
                if (databaseUser != null)
                {
                    user.Password = databaseUser.Password;
                    user.Role = databaseUser.Role;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Users.AnyAsync(e => e.UserId == user.UserId);
                if (!exists)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName", user.FacultyId);
        return View(user);
    }
}