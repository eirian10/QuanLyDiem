using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QuanLyDiem.Data;
using QuanLyDiem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Đổi từ BCrypt sang Microsoft.AspNetCore.Identity
using QuanLyDiem.Models;

namespace QuanLyDiem.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user != null)
            {
                bool isPasswordValid = false;

                // 1. Nếu mật khẩu trong DB bắt đầu bằng $2a$ -> Dùng BCrypt (Cho Admin và các GV cũ)
                if (user.Password.StartsWith("$2a$"))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
                }
                // 2. Nếu mật khẩu bắt đầu bằng AQAAAA -> Dùng Identity PasswordHasher (Cho các giảng viên mới sinh ngẫu nhiên)
                else if (user.Password.StartsWith("AQAAAA"))
                {
                    var passwordHasher = new PasswordHasher<User>();
                    var verificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                    isPasswordValid = (verificationResult == PasswordVerificationResult.Success);
                }
                // 3. Trường hợp text thô nếu có (Plaintext)
                else
                {
                    isPasswordValid = (user.Password == model.Password);
                }

                // Nếu hợp lệ thì tiến hành đăng nhập hệ thống
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

            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}