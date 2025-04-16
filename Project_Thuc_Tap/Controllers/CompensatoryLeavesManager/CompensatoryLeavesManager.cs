
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using X.PagedList.Extensions;

namespace Project_Thuc_Tap.Controllers.CompensatoryLeavesManager
{

    public class CompensatoryLeavesManagerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        public CompensatoryLeavesManagerController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;

        }
        public async Task<IActionResult> Index(DateTime? date, string? filterType, string? query, int page =1)
        {
            int pageSize = 10;
            var totalOverTime = await _context.Reports
                .Where(t => !string.IsNullOrEmpty(t.TotalOverTime)) 
                .Include(t => t.User)
                .ToListAsync(); 

            
            var filtered = totalOverTime
                .Where(t => int.TryParse(t.TotalOverTime, out var overtime) && overtime >= 8)
                .Select(t => new
                {
                    t.UserId,
                    t.TotalOverTime,
                    t.ReportDate,
                    t.User.FullName,
                })
                .ToList();

            ViewBag.TotalOverTime = filtered;
            var totalDays = await _context.CompensatoryLeaves
                 .Where(c => c.Status == true)
                 .GroupBy(c => c.UserId)
                 .Select(g =>new
                 {
                    UserId = g.Key,
                    
                    Count= g.Count(x => x.Status == true) * 8,
                 })
                 .ToListAsync();
            var result = filtered
                 .Select(f => new
                 {
                     f.UserId,
                     f.FullName,
                     f.TotalOverTime,
                     f.ReportDate,
                     LeaveCount = totalDays.FirstOrDefault(x => x.UserId == f.UserId)?.Count ?? 0
                 })
                 .ToList();

            ViewBag.Data = result;
            var listResult = _context.CompensatoryLeaves
                .Include(l=>l.User)
                .AsQueryable();
            if (date.HasValue)
            {
                listResult = listResult.Where(l=>l.CompensatoryDays == date.Value.Date);
                
            }
            if (!string.IsNullOrEmpty(filterType))
            {
                switch (filterType.ToLower())
                {
                    case "ten":
                        if (!string.IsNullOrEmpty(query))
                        {
                            listResult = listResult.Where(l => l.User.FullName != null && l.User.FullName.Contains(query));
                        }
                        break;
                    case "chuaduyet":
                        listResult = listResult.Where(l => l.Status == false);
                        break;
                }
            }    
                var PageList = listResult.OrderBy(t=>t.CompensatoryDays).ToPagedList(page, pageSize);

            return await Task.FromResult(View(PageList));
        }

        public async Task<IActionResult>ProposedLeave( CompensatoryLeave model ,string id)
        {
            var user = await _userManager.Users
                .Where(t => t.Id == id)
                .Select(t=>t.FullName ?? "Không rõ")
                .FirstOrDefaultAsync();
            // Tìm bản ghi Report theo UserId hoặc Id cụ thể
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.UserId == id); 

            if (report != null)
            {
                // Cập nhật lại giá trị TotalOverTime
                int totalOvertime = 0;
                int.TryParse(report.TotalOverTime, out totalOvertime);


                int usedLeaves = await _context.CompensatoryLeaves
                    .Where(u => u.UserId == id && u.Status == true)
                    .CountAsync();

                int remaining = totalOvertime - (usedLeaves * 8);
                report.TotalOverTime = remaining.ToString();

                // Cập nhật
                _context.Reports.Update(report);
                await _context.SaveChangesAsync();
            }
            var result = await _context.Reports
                .Where(r => r.UserId == id)
                .Select(r => r.TotalOverTime)
                .FirstOrDefaultAsync();
            int convert0 = 0;
            var convert = int.TryParse(result, out convert0 );
            if (convert0 < 8 )
            {
                TempData["No"] = $"Không thể đề xuất nghỉ bù do nhân viên này **{user}** đã hết giờ làm thêm tích luỹ!";
                return RedirectToAction("Index");
            }    
            var add = new CompensatoryLeave();
            {
                add.UserId = id;
                add.CompensatoryDays = null;
                add.Status = false;
                _context.CompensatoryLeaves.Add(add);
                await _context.SaveChangesAsync();
                
            }

            TempData["SuccessProposed"] = "Đề xuất nghỉ bù thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CreateCompensatoryLeaves(string id)
        {
            var user = await _context.DetailReports
                .Where(t=>t.UserId == id)
                .Select(t =>t.FullName)
                .FirstOrDefaultAsync();
            ViewBag.FullName = user;
           return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCompensatoryLeaves(CompensatoryLeave model, string id)
        {
            var comp = new CompensatoryLeave();
            {
                comp.UserId = id;
                comp.CompensatoryDays = model.CompensatoryDays;
                comp.Shift = model.Shift;
                comp.Status = model.Status;
                _context.CompensatoryLeaves.Add(comp);
                await _context.SaveChangesAsync();
               
            }
            
            return RedirectToAction("Index");
        }
        
        
        [HttpGet]
        public async Task<IActionResult> EditCompensatoryLeaves(int id)
        {

            var compensatoryLeave = await _context.CompensatoryLeaves
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.CompensatoryLeaveId == id);


            if (compensatoryLeave == null)
            {

                return NotFound();
            }

            return View(compensatoryLeave);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCompensatoryLeaves(CompensatoryLeave com, int id)
        {

            var result = await _context.CompensatoryLeaves.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            result.CompensatoryDays = com.CompensatoryDays;
            result.Status = com.Status;
            result.Shift = com.Shift;

            try
            {
                _context.Update(result);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật dữ liệu.");
                return View(com);
            }
        }

        public async Task<IActionResult> DeleteCompensatoryLeaves(int id)
        {
            Console.WriteLine("ID nhận được: " + id); // Kiểm tra ID

            var resultdelete = await _context.CompensatoryLeaves.FindAsync(id);
            if (resultdelete == null)
            {
                Console.WriteLine("Không tìm thấy dữ liệu với ID: " + id); // Thêm log nếu không tìm thấy
                return NotFound();
            }

            Console.WriteLine("Dữ liệu tìm thấy: " + resultdelete.CompensatoryLeaveId); // In thông tin đối tượng xóa
            _context.CompensatoryLeaves.Remove(resultdelete);
            await _context.SaveChangesAsync();

            Console.WriteLine("Đã xóa thành công ID: " + id); // Thêm log nếu xóa thành công
            return RedirectToAction("Index");
        }


    }
}
