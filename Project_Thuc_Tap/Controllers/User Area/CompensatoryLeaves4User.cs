using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;

namespace Project_Thuc_Tap.Controllers.User_Area
{
    public class CompensatoryLeaves4UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        public  CompensatoryLeaves4UserController( UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public  async Task<IActionResult> Index()
        {
            string? userId = _userManager.GetUserId(User);
            var listComp = await _context.CompensatoryLeaves
                .Include(l =>l.User)
                .Where(l => l.UserId == userId)
                .ToListAsync();

            return View(listComp);
        }
        [HttpGet]
        public async Task<IActionResult> Update4User (int id)
        {
            var result = await _context.CompensatoryLeaves
                .Include(l => l.User)
                .FirstOrDefaultAsync(l=>l.CompensatoryLeaveId == id);

            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> Update4User (int id, CompensatoryLeave model)
        {
            var update = await _context.CompensatoryLeaves
                .FindAsync(id);
            if(update !=null)
            {
                if(model.CompensatoryDays < DateTime.Now || model.CompensatoryDays ==null)
                {
                    TempData["Loi"] = "Không thể nghỉ bù cho quá khứ! Hoặc để trống !";
                   return  RedirectToAction("Update4User");
                }    
                update.UserId = _userManager.GetUserId(User);
                update.CompensatoryDays = model.CompensatoryDays;
                update.Shift = model.Shift;
                update.Status = false;
                _context.Update(update);
                await _context.SaveChangesAsync();
                TempData["SuccessUpdateComp"] = "Cập nhật thông tin nghỉ bù thành công đợi Admin duyệt nhé!";
            }    
            return RedirectToAction("Index");
        }

    }
}
