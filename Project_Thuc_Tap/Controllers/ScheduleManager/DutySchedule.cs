using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Project_Thuc_Tap.Controllers.ScheduleManager
{
    public class DutyScheduleController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DutyScheduleController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> DutySchedule(DateTime? selectedDate)
        {
            // Lấy ngày từ DatePicker hoặc mặc định là ngày hiện tại
            DateTime today = selectedDate ?? DateTime.Now;

            // Xác định ngày bắt đầu tuần (Thứ 2 là đầu tuần)
            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            DateTime endOfWeek = startOfWeek.AddDays(7);

            ViewBag.StartOfWeek = startOfWeek;
            ViewBag.SelectedDate = today.ToString("yyyy-MM-dd");

            // Lấy toàn bộ lịch trực của tuần
            var schedules = await _context.DutySchedule
                .Include(d => d.User)
                .Where(d => d.DutyDays.HasValue && d.DutyDays.Value.Date >= startOfWeek && d.DutyDays.Value.Date < endOfWeek)
                .OrderBy(d => d.DutyDays)
                .ToListAsync();

            // Gom nhóm lịch trực theo từng ngày và ca
            var groupedSchedules = schedules
        .GroupBy(d => $"{d.DutyDays.Value:yyyy-MM-dd}_{d.Shift}")
        .ToDictionary(
            g => g.Key,
            g => g.Select(ds => new
            {
                id = ds.DutyScheduleId,
                Address = ds.User.Address?? "Không biết địa chỉ của hắn",
                Description = ds.Description ?? " Không có ghi chú gì trơn khỏi đọc",
                Picture = ds.User.Picture ?? "Không có tên",
                Email = ds.User.Email ?? "Không có Email",
                FullName = ds.User.FullName ?? "Không có tên",
                Phone = ds.User.PhoneNumber ?? "Chưa có số",
                BirthDate = ds.User.BirthDate?.ToString("dd/MM/yyyy") ?? "Chưa có ngày sinh"
            }).ToList()
        );

            ViewBag.GroupedSchedules = groupedSchedules;


            return View(schedules);
        }


        

        [HttpGet]
        public async Task<IActionResult> CreateDutySchedule()
        {
            
            var userList = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    FullName = $"{u.FullName} ({u.BirthDate:dd/MM/yyyy})"
                })
                .ToListAsync();

            ViewBag.UserList = new SelectList(userList, "Id", "FullName");
            return View();
        }



        // POST: Thêm mới lịch trực
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDutySchedule(DutySchedule dutySchedule)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (dutySchedule.DutyDays < DateTime.Now.Date)
                    {
                        TempData["NotCreateDay"] = "Không thể phân công cho quá khứ!";
                        return RedirectToAction(nameof(CreateDutySchedule));
                    }

                    // Kiểm tra xem lịch trực đã tồn tại chưa
                    bool isExists = await _context.DutySchedule.AnyAsync(ds =>
                        ds.UserId == dutySchedule.UserId &&
                        ds.DutyDays == dutySchedule.DutyDays &&
                        ds.Shift == dutySchedule.Shift);

                    if (isExists)
                    {
                        TempData["DaGiaoViec"] = "Người này đã được phân công trong ca trực này!";
                        return RedirectToAction(nameof(CreateDutySchedule));
                    }

                    // Nếu chưa tồn tại, thêm mới
                    _context.Add(dutySchedule);
                    await _context.SaveChangesAsync();
                    TempData["SuccessCreateDutySchedule"] = "Tạo lịch trực thành công!";
                    return RedirectToAction(nameof(DutySchedule));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            var userList = await _userManager.Users
                .Select(u => new { u.Id, FullName = $"{u.FullName} ({u.BirthDate:dd/MM/yyyy})" })
                .ToListAsync();

            ViewBag.UserList = new SelectList(userList, "Id", "FullName");

            return View(dutySchedule);
        }


        [HttpGet]
        [Route("DeleteDutySchedule")]
        public async Task<IActionResult> DeleteDutySchedule(int id)
        {
            if (id <= 0)
            {
                return NotFound("ID không hợp lệ!");
            }

            var schedule = await _context.DutySchedule
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DutyScheduleId == id);

            if (schedule == null)
            {
                return NotFound("Không tìm thấy lịch trực!");
            }

            _context.DutySchedule.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["DeleteDutyScheduleSuccess"] = $"Đã xóa lịch trực của '{schedule.User.FullName}' thành công!";
            return RedirectToAction("DutySchedule");
        }

        [HttpGet]
        public async Task<IActionResult> EditDutySchedule(int id)
        {
            if (id < 0)
            {
                return NotFound("Không tìm thấy ID!");
            }

            // Lấy danh sách người dùng
            var userList = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    FullName = $"{u.FullName} ({u.BirthDate:dd/MM/yyyy})"
                })
                .ToListAsync();

            ViewBag.UserList = new SelectList(userList, "Id", "FullName");

            // Lấy bản ghi DutySchedule từ cơ sở dữ liệu
            var result = await _context.DutySchedule
                .FirstOrDefaultAsync(d => d.DutyScheduleId == id);
            if (result == null)
            {
                return NotFound("Lịch trực không tồn tại.");
            }

            // Trả về view với dữ liệu DutySchedule
            return View(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDutySchedule(DutySchedule dutySchedule)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bản ghi hiện tại từ cơ sở dữ liệu theo ID
                    var existingDutySchedule = await _context.DutySchedule
                        .FirstOrDefaultAsync(d => d.DutyScheduleId == dutySchedule.DutyScheduleId);

                    if (existingDutySchedule == null)
                    {
                        TempData["NotCreateDay"] = "Lịch trực không tồn tại.";
                        return RedirectToAction("DutySchedule");
                    }

                    // Cập nhật thông tin của bản ghi
                    existingDutySchedule.UserId = dutySchedule.UserId;
                    existingDutySchedule.DutyDays = dutySchedule.DutyDays;
                    existingDutySchedule.Shift = dutySchedule.Shift;
                    existingDutySchedule.Status = dutySchedule.Status;
                    existingDutySchedule.Description = dutySchedule.Description;

                    // Lưu các thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();

                    // Chuyển hướng về trang danh sách lịch trực
                    return RedirectToAction("DutySchedule");
                }
                catch (Exception ex)
                {
                    TempData["NotCreateDay"] = $"Có lỗi xảy ra: {ex.Message}";
                    return RedirectToAction("DutySchedule");
                }
            }

            // Nếu model không hợp lệ, trả lại form để sửa
            return View(dutySchedule);
        }







    }
}
