using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;

namespace Project_Thuc_Tap.Controllers.User_Area
{
    public class TimeKeepingForUserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        

        public TimeKeepingForUserController(ApplicationDbContext context, UserManager<User> userManager)
        {

            _context = context;
            _userManager = userManager;
        }
         public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> TimeKeeping4User()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);

            var model = new Project_Thuc_Tap.Models.TimeKeeping
            {
                Id = user?.Id, // Gán ID của user hiện tại
                Date = DateTime.Now.Date // Ngày hiện tại
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TimeKeeping4User(Project_Thuc_Tap.Models.TimeKeeping model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin người dùng.");
                return View(model);
            }

            var today = DateTime.Now.Date;

            // Kiểm tra ca trực hôm nay
            var existingDutySchedule = await _context.DutySchedule
                .Where(sc => sc.UserId == user.Id && sc.DutyDays == today && sc.Shift == model.Shift)
                .Select(sc => new
                {
                    sc.Shift,
                    sc.IsOverTime,
                })
                .ToListAsync();
            var isOverTime = existingDutySchedule
                .FirstOrDefault(sc => sc.Shift == model.Shift)?.IsOverTime ?? false;




            if (!existingDutySchedule.Any())
            {
                ModelState.AddModelError(string.Empty, $"Hôm nay không có ca trực cho buổi {model.Shift}");
                return View(model);
            }

            var existingTimeKeeping = await _context.TimeKeeping
                .FirstOrDefaultAsync(tk => tk.Id == user.Id && tk.Date == today && tk.Shift == model.Shift);

            if (existingTimeKeeping == null)
            {
                if (model.TimeIn == "TimeOut")
                {
                    ModelState.AddModelError(string.Empty, "Bạn cần chấm công giờ vào trước khi chấm công giờ ra.");
                    return View(model);
                }

                var timeKeeping = new Project_Thuc_Tap.Models.TimeKeeping
                {
                    Id = user.Id,
                    Date = today,
                    TimeIn = model.TimeIn == "TimeIn" ? DateTime.Now.ToString("HH:mm:ss") : null,
                    TimeOut = null,
                    Shift = model.Shift,
                    IsOverTime = isOverTime,
                    Description = model.Description
                };

                _context.TimeKeeping.Add(timeKeeping);
            }
            else
            {
                if (model.TimeIn == "TimeIn")
                {
                    if (!string.IsNullOrEmpty(existingTimeKeeping.TimeIn))
                    {
                        ModelState.AddModelError(string.Empty, "Bạn đã chấm công giờ vào hôm nay rồi!");
                        return View(model);
                    }

                    existingTimeKeeping.TimeIn = DateTime.Now.ToString("HH:mm:ss");
                }
                else if (model.TimeIn == "TimeOut")
                {
                    if (string.IsNullOrEmpty(existingTimeKeeping.TimeIn))
                    {
                        ModelState.AddModelError(string.Empty, "Bạn cần chấm công giờ vào trước khi chấm công giờ ra.");
                        return View(model);
                    }

                    if (!string.IsNullOrEmpty(existingTimeKeeping.TimeOut))
                    {
                        ModelState.AddModelError(string.Empty, "Bạn đã chấm công giờ ra hôm nay rồi!");
                        return View(model);
                    }

                    existingTimeKeeping.TimeOut = DateTime.Now.ToString("HH:mm:ss");
                }

                existingTimeKeeping.Description = model.Description;

                if (_context.Entry(existingTimeKeeping).State == EntityState.Detached)
                {
                    _context.TimeKeeping.Attach(existingTimeKeeping);
                }

                _context.TimeKeeping.Update(existingTimeKeeping);
            }

            await _context.SaveChangesAsync();
            TempData["Success4User"] = "Chấm công thành công!";

            var thongbao = await _context.TimeKeeping
                .FirstOrDefaultAsync(tk => tk.Id == user.Id && tk.Date == today);

            var catruc = await _context.DutySchedule
                .FirstOrDefaultAsync(ca => ca.UserId == user.Id && ca.DutyDays == today);

            if (thongbao != null && catruc != null)
            {
                TimeSpan gioTre, gioSom;

                switch (catruc.Shift)
                {
                    case "Sáng":
                        gioTre = new TimeSpan(6, 15, 0);
                        gioSom = new TimeSpan(14, 15, 0);
                        break;
                    case "Chiều":
                        gioTre = new TimeSpan(12, 15, 0);
                        gioSom = new TimeSpan(20, 15, 0);
                        break;
                    case "Tối":
                        gioTre = new TimeSpan(18, 15, 0);
                        gioSom = new TimeSpan(2, 0, 0);
                        break;
                    default:
                        gioTre = new TimeSpan(7, 15, 0);
                        gioSom = new TimeSpan(17, 15, 0);
                        break;
                }

                if (!string.IsNullOrEmpty(thongbao.TimeIn) && TimeSpan.TryParse(thongbao.TimeIn, out TimeSpan gioVao))
                {
                    if (gioVao > gioTre)
                    {
                        TimeSpan lateTime = gioVao - gioTre;
                        thongbao.TimeLate = lateTime.ToString(@"hh\:mm");
                        _context.TimeKeeping.Update(thongbao);
                        TempData["Warning"] = $"Bạn đã đi trễ {lateTime.TotalHours} giờ trong ca {catruc.Shift}!";
                    }
                }

                if (!string.IsNullOrEmpty(thongbao.TimeOut) && TimeSpan.TryParse(thongbao.TimeOut, out TimeSpan gioRa))
                {
                    if (catruc.Shift == "Tối" && gioRa.Hours < 12)
                    {
                        gioRa = gioRa.Add(new TimeSpan(24, 0, 0));
                    }

                    if (gioRa < gioSom)
                    {
                        TempData["Warning2"] = $"Bạn đã ra về sớm trong ca {catruc.Shift}!";
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TimeKeeping4User");
        }

    }
}

