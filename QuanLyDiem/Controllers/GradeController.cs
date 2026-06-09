using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDiem.Data;
using QuanLyDiem.Services;
using QuanLyDiem.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyDiem.Controllers
{
    [Authorize(Roles = "Lecturer")] 
    public class GradeController : Controller
    {
        private readonly IGradeService _gradeService;
        private readonly ApplicationDbContext _context;

        public GradeController(IGradeService gradeService, ApplicationDbContext context)
        {
            _gradeService = gradeService;
            _context = context;
        }

        #region ================= CHỨC NĂNG QUẢN LÝ VÀ NHẬP ĐIỂM =================

        /// <summary>
        /// CHỨC NĂNG 1: Giao diện Form nhập điểm chi tiết của Lớp học phần
        /// </summary>
        /// <param name="id">CourseClassId (ID của Lớp học phần)</param>
        [HttpGet]
        [Route("Grade/EnterScores/{id}")]
        public async Task<IActionResult> EnterScores(int id)
        {
            // 1.1. Truy vấn thông tin lớp học phần và môn học liên kết
            var courseClass = await _context.CourseClasses
                .Include(cc => cc.Subject)
                .FirstOrDefaultAsync(cc => cc.CourseClassId == id);

            // 1.2. Trả về trang lỗi 404 nếu không tìm thấy Lớp học phần phù hợp
            if (courseClass == null) return NotFound();

            // 1.3. Khởi tạo dữ liệu gửi sang View thông qua ViewModel bọc ngoài
            var viewModel = new GradebookViewModel
            {
                CourseClassId = id,
                ClassCode = courseClass.ClassCode.ToString(),
                SubjectName = courseClass.Subject.SubjectName,
                // Lấy danh sách học viên kèm theo các ô điểm hiện tại
                Students = await _gradeService.GetScoresByCourseClassIdAsync(id)
            };

            // 1.4. Đổ mã lớp học phần vào ViewBag để phục vụ nút xuất Excel tại giao diện này
            ViewBag.CourseClassId = id;

            return View(viewModel);
        }

        /// <summary>
        /// CHỨC NĂNG 2: Tiếp nhận và lưu trữ điểm số hàng loạt từ Form gửi lên
        /// </summary>
        /// <param name="model">Dữ liệu bảng điểm đã chỉnh sửa từ client</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScores(GradebookViewModel model)
        {
            // 2.1. Kiểm tra tính hợp lệ dữ liệu đầu vào (ví dụ: điểm phải từ 0 -> 10)
            if (ModelState.IsValid)
            {
                // Gọi Service xử lý cập nhật (chỉ đè những dòng dữ liệu có sự thay đổi thực sự)
                await _gradeService.UpdateScoresAsync(model.Students);

                // Sử dụng TempData để gửi thông báo xanh (Toast/Alert) sang trang chuyển hướng tiếp theo
                TempData["SuccessMessage"] = "Lưu điểm thành công!";

                // Điều hướng quay lại trang nhập điểm của chính lớp học phần này
                return RedirectToAction(nameof(EnterScores), new { id = model.CourseClassId });
            }

            // 2.2. Nếu dữ liệu không hợp lệ, trả về lại giao diện kèm thông báo lỗi cụ thể
            return View("EnterScores", model);
        }

        /// <summary>
        /// CHỨC NĂNG 3: Xuất file Excel danh sách bảng điểm phục vụ việc nhập điểm (Bảng Nhập Điểm)
        /// </summary>
        /// <param name="id">CourseClassId (ID của Lớp học phần)</param>
        [HttpGet]
        public async Task<IActionResult> ExportEnterScoresExcel(int id)
        {
            // 3.1. Truy xuất thông tin lớp học phần và danh sách sinh viên hiện tại
            var courseClass = await _context.CourseClasses.Include(cc => cc.Subject).FirstOrDefaultAsync(cc => cc.CourseClassId == id);
            var students = await _gradeService.GetScoresByCourseClassIdAsync(id);

            if (courseClass == null) return NotFound();

            // 3.2. Tiến hành khởi tạo và thiết kế cấu trúc file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Bang_Nhap_Diem");
                worksheet.Cells.Style.Font.Name = "Arial";

                // Tiêu đề văn bản lớn phía trên
                worksheet.Cells["A1"].Value = "BẢNG NHẬP ĐIỂM CHI TIẾT";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(31, 73, 125)); // Xanh đậm lam

                worksheet.Cells["A2"].Value = $"Mã lớp HP: {courseClass.ClassCode} | Học phần: {courseClass.Subject?.SubjectName}";
                worksheet.Cells["A2"].Style.Font.Size = 11;
                worksheet.Cells["A2"].Style.Font.Italic = true;

                // Tạo thanh dòng tiêu đề bảng dữ liệu (Header row) tại dòng số 4
                string[] headers = { "STT", "Mã Sinh Viên", "Họ và Tên", "Điểm Quá Trình", "Điểm Cuối Kỳ" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[4, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125));
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Duyệt danh sách đổ dữ liệu chi tiết bắt đầu từ dòng số 5
                int startRow = 5;
                for (int i = 0; i < students.Count; i++)
                {
                    worksheet.Cells[startRow, 1].Value = i + 1;
                    worksheet.Cells[startRow, 2].Value = students[i].StudentCode;
                    worksheet.Cells[startRow, 3].Value = students[i].FullName;
                    worksheet.Cells[startRow, 4].Value = students[i].ProcessScore;
                    worksheet.Cells[startRow, 5].Value = students[i].FinalScore;

                    // Định dạng cấu trúc căn lề văn bản
                    worksheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Chuẩn hóa định dạng hiển thị số thực (luôn hiển thị 1 chữ số sau dấu phẩy)
                    worksheet.Cells[startRow, 4].Style.Numberformat.Format = "0.0";
                    worksheet.Cells[startRow, 5].Style.Numberformat.Format = "0.0";

                    // Chạy vòng lặp kẻ đường viền mảnh bao quanh từng ô dữ liệu
                    for (int col = 1; col <= 5; col++)
                    {
                        worksheet.Cells[startRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Gainsboro);
                    }
                    startRow++;
                }

                // Tự động kéo dãn kích thước cột vừa vặn nội dung chữ bên trong
                worksheet.Cells[4, 1, startRow, 5].AutoFitColumns();
                worksheet.Column(3).Width = 25; // Ưu tiên nới riêng cột Họ và Tên rộng hơn

                // 3.3. Chuyển đổi dữ liệu bảng tính thành luồng Byte (Stream) và xuất file về máy client
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Bang_Nhap_Diem_{courseClass.ClassCode}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        #endregion

        #region ================= CHỨC NĂNG XEM BÁO CÁO THỐNG KÊ =================

        /// <summary>
        /// CHỨC NĂNG 4: Giao diện Xem báo cáo kết quả học tập và Thống kê GPA của lớp (Hỗ trợ lọc học lại)
        /// </summary>
        /// <param name="id">CourseClassId (ID của Lớp học phần)</param>
        /// <param name="isFailedOnly">Bộ lọc: chỉ lấy danh sách sinh viên trượt môn học lại</param>
        [HttpGet]
        public async Task<IActionResult> ViewGpaReport(int id, bool isFailedOnly = false)
        {
            // 4.1. Lấy danh sách điểm tổng kết hệ 10, hệ 4, điểm chữ quy đổi từ Service
            var gpaList = await _gradeService.GetClassGpaReportAsync(id);

            // 4.2. Thống kê số lượng từng đầu điểm chữ (A, B, C, D, F) để nạp vào đồ thị Chart.js
            var chartStats = _gradeService.GetChartStatistics(gpaList);

            // Tách mảng Key (Ký tự A, B, C...) và mảng Value (Số lượng sinh viên tương ứng) chuyển qua ViewBag
            ViewBag.LetterLabels = chartStats.Keys.ToArray();
            ViewBag.LetterData = chartStats.Values.ToArray();

            // 4.3. Bóc riêng số lượng điểm F phục vụ khối hiển thị Cảnh báo học lại tại View
            ViewBag.FailedCount = chartStats["F"];

            // 4.4. Đọc chi tiết thông tin Lớp học phần nhằm lấy nhãn Tên môn, Mã lớp ném ra Header
            var courseClass = await _context.CourseClasses
                .Include(cc => cc.Subject)
                .FirstOrDefaultAsync(cc => cc.CourseClassId == id);

            if (courseClass != null)
            {
                ViewBag.ClassCode = courseClass.ClassCode;
                ViewBag.SubjectName = courseClass.Subject?.SubjectName;
                ViewBag.CourseClassId = id; // Phục vụ truyền ID cho nút chuyển nhanh sang trang Nhập điểm hoặc xuất Excel
            }

            // 4.5. TẬN DỤNG BỘ LỌC: Gửi trạng thái lọc sang View và tiến hành lọc danh sách học lại (điểm F) nếu active
            ViewBag.IsFailedOnly = isFailedOnly;
            if (isFailedOnly)
            {
                gpaList = gpaList.Where(s => s.LetterGrade == "F").ToList();
            }

            // Trả về danh sách DTO thuần không qua trung gian ViewModel
            return View(gpaList);
        }

        /// <summary>
        /// CHỨC NĂNG 5: Xuất file thống kê báo cáo tiến độ học tập toàn lớp hoặc danh sách học lại chuyên biệt
        /// </summary>
        /// <param name="id">CourseClassId (ID của Lớp học phần)</param>
        /// <param name="isFailedOnly">Lựa chọn chỉ xuất file danh sách sinh viên học lại</param>
        [HttpGet]
        public async Task<IActionResult> ExportGpaReportExcel(int id, bool isFailedOnly = false)
        {
            // 5.1. Thu thập dữ liệu báo cáo từ database và service
            var courseClass = await _context.CourseClasses.Include(cc => cc.Subject).FirstOrDefaultAsync(cc => cc.CourseClassId == id);
            var reportData = await _gradeService.GetClassGpaReportAsync(id);

            if (courseClass == null) return NotFound();

            // 5.2. Cấu hình tiêu đề động và tiền tố tên file tùy thuộc vào chế độ in dữ liệu
            string titleName = "BẢNG BÁO CÁO TỔNG KẾT ĐIỂM GPA";
            string fileNamePrefix = "Bao_Cao_GPA";
            string sheetName = "Bao_Cao_GPA";

            if (isFailedOnly)
            {
                reportData = reportData.Where(s => s.LetterGrade == "F").ToList();
                titleName = "DANH SÁCH SINH VIÊN HỌC LẠI (ĐIỂM F)";
                fileNamePrefix = "Danh_Sach_Hoc_Lai";
                sheetName = "Hoc_Lai";
            }

            // 5.3. Khởi tạo cấu trúc bảng tính Excel báo cáo
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);
                worksheet.Cells.Style.Font.Name = "Arial";

                // Trang trí tiêu đề báo cáo lớn phía trên đầu sheet
                worksheet.Cells["A1"].Value = titleName;
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(0, 80, 115)); // Màu xanh teal đậm chuyên nghiệp

                worksheet.Cells["A2"].Value = $"Mã lớp HP: {courseClass.ClassCode} | Học phần: {courseClass.Subject?.SubjectName}";
                worksheet.Cells["A2"].Style.Font.Size = 11;
                worksheet.Cells["A2"].Style.Font.Italic = true;

                // Đổ dữ liệu Header dòng 4 (Có thêm cột Điểm Hệ 10, GPA Hệ 4, Điểm Chữ)
                string[] headers = { "STT", "Mã Sinh Viên", "Họ và Tên", "Điểm Hệ 10", "GPA Hệ 4", "Điểm Chữ" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[4, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 80, 115));
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Duyệt danh sách sinh viên đổ thông tin điểm quy đổi từ dòng 5 trở đi
                int startRow = 5;
                for (int i = 0; i < reportData.Count; i++)
                {
                    worksheet.Cells[startRow, 1].Value = i + 1;
                    worksheet.Cells[startRow, 2].Value = reportData[i].StudentCode;
                    worksheet.Cells[startRow, 3].Value = reportData[i].FullName;
                    worksheet.Cells[startRow, 4].Value = reportData[i].FinalScore10;
                    worksheet.Cells[startRow, 5].Value = reportData[i].GpaSystem4;
                    worksheet.Cells[startRow, 6].Value = reportData[i].LetterGrade;

                    // Định dạng căn chỉnh vị trí chữ
                    worksheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Định dạng hiển thị dấu thập phân số thực
                    worksheet.Cells[startRow, 4].Style.Numberformat.Format = "0.0";
                    worksheet.Cells[startRow, 5].Style.Numberformat.Format = "0.0";

                    // TÍNH NĂNG 6: Tô màu Highlight phân loại tự động tại cột Điểm Chữ trên file Excel
                    var letterCell = worksheet.Cells[startRow, 6];
                    letterCell.Style.Font.Bold = true;

                    if (reportData[i].LetterGrade == "A")
                    {
                        letterCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        letterCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 240, 217)); // Tô nền màu Xanh lá nhạt dịu mắt
                    }
                    else if (reportData[i].LetterGrade == "F")
                    {
                        letterCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        letterCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(252, 228, 214)); // Tô nền màu Đỏ hồng nhạt cảnh báo
                    }

                    // Đóng khung viền mảnh cho toàn bộ dòng dữ liệu hiện hành
                    for (int col = 1; col <= 6; col++)
                    {
                        worksheet.Cells[startRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Gainsboro);
                    }
                    startRow++;
                }

                // Tự căn chỉnh co giãn các cột dữ liệu theo nội dung chữ thực tế
                worksheet.Cells[4, 1, startRow, 6].AutoFitColumns();
                worksheet.Column(3).Width = 25; // Khóa rộng cột Họ tên để tránh bị vỡ hàng dữ liệu

                // 5.4. Nén file lưu vào bộ nhớ đệm và phản hồi lệnh tải tập tin xuống cho trình duyệt khách
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"{fileNamePrefix}_{courseClass.ClassCode}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        #endregion
    }
}