using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;

namespace Project_Thuc_Tap.Controllers.User_Area
{
    public class DutyScheduleForUserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public DutyScheduleForUserController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> DutySchedule4User(DateTime? selectedDate)
        {
            // Lấy userId của người đăng nhập
            string userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Nếu không tìm thấy user, trả về lỗi 401
            }

            // Lấy ngày từ DatePicker hoặc mặc định là ngày hiện tại
            DateTime today = selectedDate ?? DateTime.Now;

            // Xác định ngày bắt đầu tuần (Thứ 2 là đầu tuần)
            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            DateTime endOfWeek = startOfWeek.AddDays(7);

            ViewBag.StartOfWeek = startOfWeek;
            ViewBag.SelectedDate = today.ToString("yyyy-MM-dd");

            // Lấy lịch trực của user trong tuần hiện tại
            var schedules = await _context.DutySchedule
                .Include(d => d.User)
                .Where(d => d.UserId == userId && d.DutyDays.HasValue && d.DutyDays.Value.Date >= startOfWeek && d.DutyDays.Value.Date < endOfWeek)
                .OrderBy(d => d.DutyDays)
                .ToListAsync();

            // Gom nhóm lịch trực theo ngày và ca trực
            var groupedSchedules = schedules
                .GroupBy(d => $"{d.DutyDays.Value:yyyy-MM-dd}_{d.Shift}")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ds => new
                    {
                        id = ds.DutyScheduleId,
                        Address = ds.User.Address ?? "Không biết địa chỉ",
                        Description = ds.Description ?? "Không có ghi chú",
                        Picture = ds.User.Picture ?? "/Images/avatar-default.png",
                        Email = ds.User.Email ?? "Không có Email",
                        FullName = ds.User.FullName ?? "Không có tên",
                        Phone = ds.User.PhoneNumber ?? "Chưa có số điện thoại",
                        BirthDate = ds.User.BirthDate?.ToString("dd/MM/yyyy") ?? "Chưa có ngày sinh",
                        Status = ds.Status,
                        IsOverTime = ds.IsOverTime,

                    }).ToList()
                );

            //Lấy lịch nghỉ bù
                    var compensatoryleaves = await _context.CompensatoryLeaves
            .Include(d => d.User)
            .Where(d => d.UserId == userId && d.CompensatoryDays.HasValue && d.CompensatoryDays.Value.Date >= startOfWeek && d.CompensatoryDays.Value.Date < endOfWeek)
            .OrderBy(d => d.CompensatoryLeaveId)
            .ToListAsync();
            var groupedcompensatory = compensatoryleaves
                 .GroupBy(d => $"{d.CompensatoryDays.Value:yyyy-MM-dd}_{d.Shift}")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ds => new
                    {
                        id = ds.CompensatoryLeaveId,
                        Address = ds.User.Address ?? "Không biết địa chỉ",
                        Picture = ds.User.Picture ?? "/Images/avatar-default.png",
                        Email = ds.User.Email ?? "Không có Email",
                        FullName = ds.User.FullName ?? "Không có tên",
                        Phone = ds.User.PhoneNumber ?? "Chưa có số điện thoại",
                        BirthDate = ds.User.BirthDate?.ToString("dd/MM/yyyy") ?? "Chưa có ngày sinh"
                    }).ToList()
                );

            //Hiển thị
            ViewBag.GroupedSchedules = groupedSchedules;
            ViewBag.Compensatory = groupedcompensatory;

            return View(schedules);
        }

    }
}
