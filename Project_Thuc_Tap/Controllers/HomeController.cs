using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using System.Diagnostics;

namespace Project_Thuc_Tap.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context= context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewAdmin()
        {

            return View();
        }

        public async Task<IActionResult> ViewUser(DateTime? selectedDate)
        {
            
            var U = await _userManager.GetUserAsync(User);
            string UserId = U.Id;
            if (UserId == null)
            {
                return NotFound();
            }
            else
            {
                var dutyToDay = await _context.DutySchedule
                    .Where(d=>d.UserId == UserId && d.DutyDays == DateTime.Now.Date)
                    .CountAsync();
                ViewBag.DutyToDay = dutyToDay;
                var TimeKeepingToDay = await (from tk in _context.TimeKeeping
                                              join ds in _context.DutySchedule
                                              on tk.Id equals ds.UserId
                                              where tk.Date == ds.DutyDays && ds.DutyDays == DateTime.Now.Date && ds.Shift == tk.Shift
                                              select new
                                              {
                                                  tk.TimeIn,
                                                  tk.TimeOut,
                                                  tk.TimeLate,
                                                  tk.Date,
                                                  tk.Shift,
                                              }).ToListAsync();
                var today = DateTime.Today;
                var timeMessages = new List<string>();
                var noMessages = new List<string>();
                var timeOutMessages = new List<string>();
                var timeLateMessages = new List<string>();

                foreach (var item in TimeKeepingToDay)
                {
                    var hasIn = !string.IsNullOrEmpty(item.TimeIn);
                    var hasOut = !string.IsNullOrEmpty(item.TimeOut);
                    var hasLate = !string.IsNullOrEmpty(item.TimeLate);
                    var no = string.IsNullOrEmpty(item.TimeIn) && string.IsNullOrEmpty(item.TimeOut);
                    if (hasIn && hasOut)
                    {
                        timeMessages.Add($"✅ Đã chấm công đầy đủ cho ca {item.Shift} vào ngày {item.Date:dd/MM/yyyy}.");
                    }

                    if (hasIn && !hasOut)
                    {
                        timeOutMessages.Add($"⚠️ Đã chấm công giờ vào nhưng chưa chấm công giờ ra cho ca {item.Shift} vào ngày {item.Date:dd/MM/yyyy}.");
                    }

                    if (hasLate)
                    {
                        timeLateMessages.Add($"⏰ Bạn đã đi trễ {item.TimeLate} giờ trong ca {item.Shift} vào ngày {item.Date:dd/MM/yyyy}.");
                    }

                    if (no)
                    {
                        noMessages.Add($"❌ Chưa chấm công cho ca {item.Shift} vào ngày {item.Date:dd/MM/yyyy}.");
                    }
                }

                // Gửi ra view
                ViewBag.TimeMessages = timeMessages;
                ViewBag.NoMessages = noMessages;
                ViewBag.TimeOutMessages = timeOutMessages;
                ViewBag.TimeLateMessages = timeLateMessages;







            }
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
