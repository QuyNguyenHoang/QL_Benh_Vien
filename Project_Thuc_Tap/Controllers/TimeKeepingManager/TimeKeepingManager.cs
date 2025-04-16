
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using X.PagedList.Extensions;

namespace Project_Thuc_Tap.Controllers.TimeKeepingManager
{
    public class TimeKeepingManagerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public TimeKeepingManagerController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index( string? filterType, DateTime? date, string? query, int page = 1)
        {
            int pageSize = 10;
            var queryable = _context.TimeKeeping
                .Include(tk => tk.User).AsQueryable();
                
            if (date.HasValue)
            {
                queryable = queryable.Where(q => q.Date == date);
            }

            if (!string.IsNullOrEmpty(filterType))
            {
                switch (filterType.ToLower())
                {

                    case "ten":
                        if (!string.IsNullOrEmpty(query))
                        {
                            queryable = queryable.Where(q => q.User.FullName != null && q.User.FullName.Contains(query));
                        }
                        break;

                }
            }
            var PageList = queryable.OrderBy(t=>t.Id).ToPagedList(page, pageSize);
            return await Task.FromResult(View(PageList));
        }
        [HttpGet]
        public async Task<IActionResult> EditTimeKeeping(int id)
        {
            var timeKeeping = await _context.TimeKeeping
                .Include(tk => tk.User)
                .FirstOrDefaultAsync(tk => tk.TimeKeepingId == id);

            if (timeKeeping == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy dữ liệu
            }

            return View(timeKeeping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTimeKeeping(Project_Thuc_Tap.Models.TimeKeeping model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả về View nếu dữ liệu không hợp lệ
            }

            var timeKeeping = await _context.TimeKeeping.FindAsync(model.TimeKeepingId);
            if (timeKeeping == null)
            {
                return NotFound();
            }

            // Cập nhật dữ liệu từ form
            timeKeeping.Date = model.Date;
            timeKeeping.TimeIn = model.TimeIn;
            timeKeeping.TimeOut = model.TimeOut;
            timeKeeping.Description = model.Description;

            try
            {
                _context.Update(timeKeeping);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index"); // Quay lại danh sách sau khi cập nhật thành công
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật dữ liệu.");
                return View(model);
            }
        }
        public async Task<IActionResult> DeleteTimeKeeping(int id)
        {
            var timeKeeping = await _context.TimeKeeping.FindAsync(id);
            if (timeKeeping == null) return NotFound();

            _context.TimeKeeping.Remove(timeKeeping);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


    }

}
