using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Signing;
using OfficeOpenXml;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using System.ComponentModel;
using System.Security.Cryptography;
using X.PagedList.Extensions;
using LicenseContext = OfficeOpenXml.LicenseContext;



namespace Project_Thuc_Tap.Controllers.ReportManager
{
    public class ReportManagerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        public ReportManagerController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var result = await _context.Reports
                .Include(r => r.User)
                .OrderBy(r => r.ReportId)
                .ToListAsync();
            var PageList = result.ToPagedList(page, pageSize);
            return View(PageList);
        }

        public async Task<IActionResult> UpdateReports(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn khoảng thời gian hợp lệ.";
                return RedirectToAction("Index");
            }
            if (toDate > DateTime.Now)
            {
                TempData["ErrorDate"] = "Không thể thống kê cho tương lai! Thống kê đến hiện tại";
                toDate = DateTime.Now;
            }

            var listUser = await _userManager.Users.ToListAsync();

            foreach (var user in listUser)
            {
                var timeKeeping = await _context.TimeKeeping
                    .Where(t => t.Id == user.Id && t.Date >= fromDate && t.Date <= toDate)
                    .Select(t => t.Date)
                    .ToListAsync();

                var totalWorkShift = await _context.DutySchedule
                    .Where(ds => ds.UserId == user.Id && ds.Status == true && ds.DutyDays.HasValue
                                 && ds.DutyDays.Value.Date >= fromDate && ds.DutyDays.Value.Date <= toDate
                                 && timeKeeping.Contains(ds.DutyDays.Value.Date))
                    .Select(ds => ds.Shift)
                    .CountAsync();

                int totalWorkHours = totalWorkShift * 8;

                var OverTime = await _context.DutySchedule
                    .Where(t => t.UserId == user.Id && t.IsOverTime == true && t.DutyDays.HasValue
                                && t.DutyDays.Value.Date >= fromDate && t.DutyDays.Value.Date <= toDate
                                && timeKeeping.Contains(t.DutyDays.Value.Date))
                    .Select(t => t.Shift)
                    .CountAsync();
                var totalOverTime = OverTime * 8;
                var totalCompensatoryLeaves = await _context.CompensatoryLeaves
                    .Where(t => t.UserId == user.Id && t.Status == true
                                && t.CompensatoryDays >= fromDate && t.CompensatoryDays <= toDate)
                    .Select(t => t.CompensatoryDays)
                    .CountAsync();

                var totalLate = _context.TimeKeeping
                    .Where(t => t.Id == user.Id && !string.IsNullOrEmpty(t.TimeLate)
                                && t.Date >= fromDate && t.Date <= toDate)
                    .AsEnumerable()
                    .Select(t => TimeSpan.Parse(t.TimeLate).TotalMinutes)
                    .Sum();

                var existingReport = await _context.Reports
                    .FirstOrDefaultAsync(r => r.UserId == user.Id);

                if (existingReport != null)
                {
                    existingReport.TotalOverTime = totalOverTime.ToString();
                    existingReport.TotalWorkHours = totalWorkHours.ToString();
                    existingReport.ReportDate = $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";
                    existingReport.TotalCompensatoryDays = totalCompensatoryLeaves;
                    existingReport.TotalLateHours = totalLate.ToString();
                    _context.Reports.Update(existingReport);
                }
                else
                {
                    var newReport = new Report
                    {
                        UserId = user.Id,
                        TotalWorkHours = totalWorkHours.ToString(),
                        ReportDate = $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}",
                        TotalOverTime = totalOverTime.ToString(),
                        TotalCompensatoryDays = totalCompensatoryLeaves,
                        TotalLateHours = totalLate.ToString(),
                    };
                    _context.Reports.Add(newReport);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật báo cáo thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DetailReports(string id, DateTime? FromDate, DateTime? ToDate, int page = 1)
        {
            int pageSize = 10;
            var listDetail = await _context.TimeKeeping
                .Where(l => l.Id == id)
                .Include(l => l.User)
                .Select(l => l.Date)
                .Distinct()
                .ToListAsync();

            foreach (var workingDate in listDetail)
            {
                // Kiểm tra xem ngày này đã có trong bảng DetailReports chưa
                var existingReport = await _context.DetailReports
                    .FirstOrDefaultAsync(r => r.UserId == id
                                           && r.WorkingDay.HasValue
                                           && r.WorkingDay.Value.Date == workingDate);
                var In4User = await _userManager.Users
                        .Where(u => u.Id == id)
                        .FirstOrDefaultAsync();
                var Shift = await _context.TimeKeeping
                    .Where(t => t.Id == id && t.Date == workingDate)
                    .Select(t => t.Shift)
                    .Distinct()
                    .CountAsync();
                var totalShift = Shift * 8;
                var OverTime = await _context.TimeKeeping
                    .Where(t => t.Id == id && t.IsOverTime == true && t.Date == workingDate)
                    .Distinct()
                    .CountAsync();
                var totalOverTime = OverTime * 8;
                var totalLate = _context.TimeKeeping
                   .Where(t => t.Id == id && t.Date == workingDate && !string.IsNullOrEmpty(t.TimeLate))
                   .AsEnumerable()
                   .Select(t => TimeSpan.Parse(t.TimeLate).TotalMinutes)
                   .Sum();
                var totalCompensatoryLeaves = await _context.CompensatoryLeaves
                   .Where(t => t.UserId == id && t.CompensatoryDays == workingDate && t.Status == true)
                   .Select(t => t.CompensatoryDays)
                   .CountAsync();



                if (existingReport == null)
                {

                    var newReport = new DetailReports
                    {
                        UserId = id,
                        FullName = In4User?.FullName,
                        BirthDate = In4User?.BirthDate,
                        TotalWorkHours = totalShift.ToString(),
                        TotalOverTime = totalOverTime.ToString(),
                        TotalLate = totalLate.ToString(),
                        WorkingDay = workingDate,
                        TotalCompensatoryLeave = totalCompensatoryLeaves.ToString(),
                        Notes = "Tự động thêm mới",

                    };
                    _context.DetailReports.Add(newReport);
                }
                else
                {
                    existingReport.FullName = In4User?.FullName;
                    existingReport.BirthDate = In4User?.BirthDate;
                    existingReport.Notes = "Tự động cập nhật";
                    existingReport.TotalWorkHours = totalShift.ToString();
                    existingReport.TotalOverTime = totalOverTime.ToString();
                    existingReport.TotalLate = totalLate.ToString();
                    existingReport.TotalCompensatoryLeave = totalCompensatoryLeaves.ToString();
                    _context.DetailReports.Update(existingReport);
                }
            }

            await _context.SaveChangesAsync();
            var delete = await _context.DetailReports
                .Where(d => d.UserId == id
                && d.WorkingDay.HasValue
                && !listDetail.Contains(d.WorkingDay.Value.Date))
                .ToListAsync();
            if (delete.Any())
            {
                _context.RemoveRange(delete);
                await _context.SaveChangesAsync();
            }
            if (FromDate.HasValue && ToDate.HasValue)
            {
                var filterForDay = await _context.DetailReports
                    .Where(r => r.UserId == id && r.WorkingDay >= FromDate && r.WorkingDay <= ToDate)
                    .OrderBy(t => t.WorkingDay)
                    .ToListAsync();
                var PageList = filterForDay.ToPagedList(page, pageSize);
                return View(PageList);
            }
            else if (FromDate.HasValue || ToDate.HasValue)
            {
                var FromOrTo = await _context.DetailReports
                    .Where(t => t.UserId == id && (t.WorkingDay == FromDate || t.WorkingDay == ToDate))
                     .OrderBy(t => t.WorkingDay)
                    .ToListAsync();
                var PageList = FromOrTo.ToPagedList(page, pageSize);
                return View(PageList);
            }
            else
            {
                var result = await _context.DetailReports
                .Where(r => r.UserId == id)
                .OrderBy(t => t.WorkingDay)
                .ToListAsync();
                var PageList = result.ToPagedList(page, pageSize);

                return View(PageList);
            }


        }
        [HttpGet("/searchReport")]
        public async Task<IActionResult> SearchReport(string? filterType, string query, int page = 1)
        {
            int pageSize = 10;
            var reports = _context.Reports.AsQueryable();

            if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(query))
            {
                switch (filterType.ToLower())
                {
                    case "ten":
                        reports = reports
                            .Join(_context.Users,
                                  report => report.UserId,
                                  user => user.Id,
                                  (report, user) => new { Report = report, User = user })
                            .Where(joined => joined.User.FullName.Contains(query))
                            .Select(joined => joined.Report);
                        break;
                }
            }
            else
            {
                reports = reports.Where(r => true);
            }
            var count = reports.Count();
            ViewBag.Count = count;
            ViewBag.Query = query;
            var searchResults = await reports
                .Include(nv => nv.User)
                .OrderBy(nv => nv.ReportId)
                .ToListAsync();
            var PageList = searchResults.ToPagedList(page, pageSize);
            return View("SearchReport", PageList);
        }
        public async Task<IActionResult> DeleteReports (int id)
        {
            var delete = await _context.Reports
                .Where(d=>d.ReportId == id)
                .FirstOrDefaultAsync();
            _context.Reports.Remove(delete);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ExportReports ()
        {
            var resultReport = await _context.Reports
                .Include(nv => nv.User)
                .ToListAsync();
            if(resultReport == null)
            {
                return BadRequest("Không có dữ liệu");
            }  
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Báo cáo");
                worksheet.Cell(1, 1).Value = "Số thứ tự";
                worksheet.Cell(1, 2).Value = "Ngày tháng";
                worksheet.Cell(1, 3).Value = "Tên";
                worksheet.Cell(1, 4).Value = "Tổng giờ làm";
                worksheet.Cell(1, 5).Value = "Tổng giờ làm thêm";
                worksheet.Cell(1, 6).Value = "Tổng ngày nghỉ";
                worksheet.Cell(1, 7).Value = "Tổng giờ đi trễ";
                worksheet.Cell(1, 8).Value = "Ghi chú";

                int stt = 1;
                int row = 2;
                foreach (var report in resultReport)
                {
                   
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = report.ReportDate;
                    worksheet.Cell(row, 3).Value = string.IsNullOrEmpty(report.User?.FullName)?"Không rõ":report.User.FullName;
                    worksheet.Cell(row, 4).Value = report.TotalWorkHours;
                    worksheet.Cell(row, 5).Value = report.TotalOverTime;
                    worksheet.Cell(row, 6).Value = report.TotalCompensatoryDays;
                    worksheet.Cell(row, 7).Value = report.TotalLateHours;
                    worksheet.Cell(row, 8).Value = report.Notes;
                    row++;

                }
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                var filename = "Report.xlsx";
                return File(stream , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            }    
            
        }
        public async Task<IActionResult> ExportToExcel(string id, DateTime? FromDate , DateTime? ToDate)
        {
            // Lấy danh sách dữ liệu từ cơ sở dữ liệu
            var result = _context.DetailReports.AsQueryable();
            if (FromDate.HasValue && ToDate.HasValue)
            {
                result = result.Where(t => t.UserId == id && t.WorkingDay >= FromDate && t.WorkingDay <= ToDate);
            }    
            else if (ToDate.HasValue)
            {
                result = result.Where(t=>t.UserId==id &&   t.WorkingDay == ToDate);
            }
            else
            {
                result = result.Where(t=>t.UserId== id);
            }
            var details = await result.ToListAsync();
            // Đảm bảo danh sách 'details' có dữ liệu
            if (details == null || !details.Any())
            {
                return BadRequest("Không có dữ liệu để xuất.");
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Chi Tiết Báo Cáo");

                // Thêm tiêu đề cho các cột
                worksheet.Cell(1, 1).Value = "Số thứ tự";
                worksheet.Cell(1, 2).Value = "Họ tên";
                worksheet.Cell(1, 3).Value = "Ngày sinh";
                worksheet.Cell(1, 4).Value = "Ngày làm việc";
                worksheet.Cell(1, 5).Value = "Tổng giờ làm";
                worksheet.Cell(1, 6).Value = "Tổng giờ làm thêm";
                worksheet.Cell(1, 7).Value = "Tổng ngày nghỉ bù";
                worksheet.Cell(1, 8).Value = "Tổng giờ đi trễ";
                worksheet.Cell(1, 9).Value = "Ghi chú";

                // Điền dữ liệu vào các dòng
                int row = 2;
                int stt = 1;
                foreach (var detail in details)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = detail.FullName; 
                    worksheet.Cell(row, 3).Value = detail.BirthDate;
                    worksheet.Cell(row, 4).Value = detail.WorkingDay;
                    worksheet.Cell(row, 5).Value = detail.TotalWorkHours;
                    worksheet.Cell(row, 6).Value = detail.TotalOverTime;
                    worksheet.Cell(row, 7).Value = detail.TotalCompensatoryLeave;
                    worksheet.Cell(row, 8).Value = detail.TotalLate;
                    worksheet.Cell(row, 9).Value = detail.Notes;
                    row++;
                }
               
                // Tạo MemoryStream và lưu file Excel vào stream
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                // Trả về file Excel cho người dùng
                var fileName = "DanhSachChiTietBaiBaoCao.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }






    }
}
