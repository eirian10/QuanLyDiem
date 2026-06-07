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
    // Khai báo các dịch vụ (Service) dùng chung trong toàn bộ Controller
    private readonly ApplicationDbContext _context; // Dịch vụ kết nối và tương tác với Database
    private readonly EmailService _emailService;   // Dịch vụ hỗ trợ gửi Email tự động

    // Constructor (Hàm khởi tạo): Nơi .NET tự động "bơm" các dịch vụ từ hệ thống vào đây để sẵn sàng sử dụng
    public LecturersController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // ==========================================================
    // 1. TRANG DANH SÁCH & TÌM KIẾM GIẢNG VIÊN (MẶC ĐỊNH)
    // ==========================================================
    public async Task<IActionResult> Index(string searchString)
    {
        // Bước 1: Chuẩn bị câu lệnh truy vấn lấy ra các User có vai trò là "Lecturer"
        // .Include(u => u.Faculty) giúp nạp kèm dữ liệu của bảng Khoa (tránh bị lỗi null khi gọi tên Khoa ở View)
        var lecturers = _context.Users
            .Include(u => u.Faculty)
            .Where(u => u.Role == "Lecturer");

        // Bước 2: Nếu người dùng có nhập từ khóa vào ô tìm kiếm trên giao diện
        if (!string.IsNullOrEmpty(searchString))
        {
            // Tiến hành lọc tiếp các giảng viên có Tên, Email hoặc Tên khoa chứa từ khóa tìm kiếm
            lecturers = lecturers.Where(u => u.FullName.Contains(searchString)
                                          || u.Email.Contains(searchString)
                                          || u.Faculty!.FacultyName.Contains(searchString));
        }

        // Bước 3: Đẩy câu lệnh xuống Database để lấy dữ liệu lên (.ToListAsync) và truyền danh sách đó ra ngoài View hiển thị
        return View(await lecturers.ToListAsync());
    }

    // ==========================================================
    // 2. HIỂN THỊ FORM THÊM MỚI GIẢNG VIÊN (GET)
    // ==========================================================
    [HttpGet]
    public IActionResult Create()
    {
        // Bốc danh sách các Khoa từ DB lên, nạp vào ViewBag dưới dạng SelectList (gồm ID và Tên Khoa) 
        // để ngoài giao diện hiển thị thành một menu thả xuống (Dropdown List) cho người dùng chọn nhanh
        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
        return View();
    }

    // ==========================================================
    // 3. XỬ LÝ LƯU GIẢNG VIÊN MỚI VÀO DATABASE (POST)
    // ==========================================================
    [HttpPost]
    [ValidateAntiForgeryToken] // Chốt chặn bảo mật chống tấn công giả mạo yêu cầu gửi từ một trang web khác
    public async Task<IActionResult> Create(User user)
    {
        // Ép buộc thuộc tính quyền (Role) luôn luôn là "Lecturer" vì đây là form tạo riêng cho giảng viên
        user.Role = "Lecturer";

        // Kiểm tra nghiệp vụ: Xem Tên đăng nhập (Username) nhập vào đã bị trùng với ai trong hệ thống chưa
        bool isUsernameExist = await _context.Users.AnyAsync(u => u.Username == user.Username);
        if (isUsernameExist)
        {
            ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại trong hệ thống. Vui lòng chọn tên khác.");
        }

        // Kiểm tra nghiệp vụ: Xem Email nhập vào đã được đăng ký cho tài khoản nào khác chưa
        bool isEmailExist = await _context.Users.AnyAsync(u => u.Email == user.Email);
        if (isEmailExist)
        {
            ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng. Vui lòng nhập email khác.");
        }

        // Tự động sinh một chuỗi mật khẩu ngẫu nhiên dài 10 ký tự bằng hàm phụ ở phía dưới
        string rawPassword = GenerateRandomPassword();

        // Tiến hành băm (mã hóa) mật khẩu thô thành chuỗi ký tự bảo mật cao trước khi lưu vào Database
        user.Password = new PasswordHasher<User>().HashPassword(user, rawPassword);

        // ModelState.IsValid: Kiểm tra xem toàn bộ form gửi lên có vượt qua tất cả các chốt chặn validation (như bỏ trống, sai định dạng...) không
        if (ModelState.IsValid)
        {
            _context.Add(user);             // Đưa đối tượng user mới vào bộ theo dõi dữ liệu của Entity Framework
            await _context.SaveChangesAsync(); // Chính thức lưu dữ liệu xuống các hàng trong bảng cơ sở dữ liệu

            // Gửi email thông báo tự động chứa tài khoản và mật khẩu thô về hòm thư của giảng viên mới vừa được tạo
            await _emailService.SendEmailAsync(user.Email, "Thông tin tài khoản giảng viên",
                $"Chào bạn, tài khoản của bạn đã được tạo.\nUsername: {user.Username}\nMật khẩu: {rawPassword}");

            // Lưu một thông báo thành công tạm thời để hiển thị ở trang tiếp theo khi chuyển hướng
            TempData["Success"] = "Đã thêm giảng viên và gửi thông tin qua email!";
            return RedirectToAction(nameof(Index)); // Điều hướng người dùng quay trở lại trang danh sách giảng viên chính
        }

        // Nếu dữ liệu nhập vào form có lỗi, nạp lại menu thả xuống các Khoa và trả lại dữ liệu đang nhập lỗi về form để người dùng sửa tiếp
        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName", user.FacultyId);
        return View(user);
    }

    // ==========================================================
    // 4. XỬ LÝ XÓA GIẢNG VIÊN (POST) - ĐÃ ĐƯỢC BỌC LỖI RÀNG BUỘC DB
    // ==========================================================
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

    // ==========================================================
    // 5. HIỂN THỊ FORM CHỈNH SỬA THÔNG TIN GIẢNG VIÊN (GET)
    // ==========================================================
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound(); // Chặn lỗi bảo mật: Nếu trên thanh địa chỉ URL không truyền ID lên thì báo lỗi không tìm thấy luôn

        // Tìm kiếm chính xác hàng dữ liệu có UserId trùng khớp và dòng đó bắt buộc phải mang Role là "Lecturer"
        var lecturer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Lecturer");
        if (lecturer == null) return NotFound(); // Nếu tìm dưới DB không có giảng viên nào khớp thông tin thì báo lỗi trang

        // Nạp danh sách các Khoa vào ViewBag để hiển thị menu Dropdown, đồng thời chọn sẵn (Select) Khoa hiện tại của giảng viên đó
        ViewBag.Faculties = new SelectList(_context.Faculties, "FacultyId", "FacultyName", lecturer.FacultyId);
        return View(lecturer); // Đẩy toàn bộ dữ liệu của giảng viên này ra View để điền sẵn vào các ô nhập liệu trên Form sửa
    }

    // ==========================================================
    // 6. XỬ LÝ LƯU DỮ LIỆU GIẢNG VIÊN SAU KHI SỬA (POST)
    // ==========================================================
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