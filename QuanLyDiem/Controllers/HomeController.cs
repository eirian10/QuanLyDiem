using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Models;

namespace QuanLyDiem.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalStudents = await _context.Users.Where(u => u.Role == "Student").CountAsync();
        ViewBag.TotalTeachers = await _context.Users.Where(u => u.Role == "Teacher").CountAsync();
        ViewBag.TotalClasses = await _context.HomeroomClasses.CountAsync();
        ViewBag.TotalFaculties = await _context.Faculties.CountAsync();
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
