using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QuanLyDiem.Data;
using QuanLyDiem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuanLyDiem.Models;
using QuanLyDiem.Services;

namespace QuanLyDiem.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user != null)
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

                if (isPasswordValid)
                {
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("UserId", user.UserId.ToString())
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }
            }

            // Chỉ chạy dòng này nếu user null hoặc mật khẩu sai
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie xác thực của người dùng
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa luôn Session 
            HttpContext.Session.Clear();

            // Chuyển hướng về trang Đăng nhập
            return RedirectToAction("Login", "Auth");
        }

        // --- TÍNH NĂNG QUÊN MẬT KHẨU (MVC) ---

        // Hiển thị form nhập Email
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Kiểm tra đầu vào
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập địa chỉ email.";
                return View();
            }

            // 1. Kiểm tra email có tồn tại trong DB không
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Trả về thông báo lỗi nếu email không tồn tại
                ViewBag.Error = "Email này không tồn tại trong hệ thống.";
                return View();
            }

            // 2. Nếu tồn tại, mới tạo OTP và gửi mail
            string otp = new Random().Next(100000, 999999).ToString();

            // Lưu vào Session để dùng cho bước VerifyOtp
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Email", email);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.Now.AddMinutes(5).Ticks.ToString());

            // Gửi mail
            await _emailService.SendEmailAsync(email, "Mã xác thực OTP - EduGrade", $"Mã xác thực của bạn là: {otp}. Mã có hiệu lực trong 5 phút.");

            return RedirectToAction("VerifyOtp");
        }

        [AllowAnonymous]
        // Hiển thị form nhập OTP
        [HttpGet]
        public IActionResult VerifyOtp() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(string otp)
        {
            var savedOtp = HttpContext.Session.GetString("OTP");
            var expiryTicks = HttpContext.Session.GetString("OTP_Expiry");

            // Kiểm tra null trước khi sử dụng để tránh lỗi
            if (string.IsNullOrEmpty(savedOtp) || string.IsNullOrEmpty(expiryTicks))
            {
                ViewBag.Error = "Phiên làm việc đã hết hạn.";
                return View();
            }

            // Sử dụng long.TryParse để chuyển đổi an toàn
            if (long.TryParse(expiryTicks, out long expiry))
            {
                if (savedOtp == otp && DateTime.Now.Ticks < expiry)
                {
                    return RedirectToAction("ResetPassword");
                }
            }

            ViewBag.Error = "Mã OTP không đúng hoặc đã hết hạn.";
            return View();
        }

        // Hiển thị form đặt mật khẩu mới
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string newPassword)
        {
            // Lấy email từ Session lúc nhập OTP
            var email = HttpContext.Session.GetString("OTP_Email");

            // Nếu bị mất session (để máy quá lâu)
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Phiên làm việc đã hết hạn. Vui lòng làm lại từ đầu.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                // 1. Hash mật khẩu bằng BCrypt
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

                // 2. Lưu xuống Database
                _context.Users.Update(user); // Thêm dòng này cho chắc
                await _context.SaveChangesAsync();

                // 3. Xóa session cho an toàn
                HttpContext.Session.Clear();

                // 4. CHÚ Ý: Chuyển trang thì phải dùng TempData mới hiện được thông báo!
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Hãy đăng nhập lại!";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Error = "Tài khoản không tồn tại.";
            return View();
        }

        // Bắt đầu luồng Google
        [AllowAnonymous]
        [HttpGet]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        // Google callback về đây
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded)
            {
                TempData["Error"] = "Đăng nhập Google thất bại.";
                return RedirectToAction("Login");
            }

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                TempData["Error"] = "Không lấy được email từ Google.";
                return RedirectToAction("Login");
            }

            // Chỉ tìm, không tạo mới
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Email này không có trong hệ thống. Vui lòng liên hệ Admin.";
                return RedirectToAction("Login");
            }

            // Tạo cookie giống login thường
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserId.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
    }
}