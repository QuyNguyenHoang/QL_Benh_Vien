using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;

namespace Project_Thuc_Tap.Controllers.User_Area
{
    public class InfController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        public InfController(UserManager<User> userManager,ApplicationDbContext context) 
        { 
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            string UserId =  _userManager.GetUserId(User)?? "";
            if (UserId == null)
            {
                return NotFound();
            }
            else
            {
                var inf = await _userManager.Users
                    .Include(t=>t.Room)
                    .Where(t=>t.Id == UserId)
                    .FirstOrDefaultAsync();
                return View(inf);
            }
        }
    }
}
