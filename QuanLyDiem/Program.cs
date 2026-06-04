using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký service theo phạm vi một request
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<CourseClassManagementService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
});

builder.Services.AddScoped<IGradeService, GradeService>();

OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("QuanLyDiem");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();