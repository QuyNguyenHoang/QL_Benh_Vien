using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using X.PagedList;
using X.PagedList.Extensions;
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

        public async Task<IActionResult> Index(DateTime? date, string? filterType, string? query, int page = 1)
        {
            int pageSize = 10;
            var queryable = _context.DutySchedule.Include(l => l.User).AsQueryable();

            if (date.HasValue)
            {
                queryable = queryable.Where(l => l.DutyDays.HasValue && l.DutyDays.Value.Date == date.Value.Date);
            }

            if (!string.IsNullOrEmpty(filterType))
            {
                switch (filterType.ToLower())
                {
                    case "ten":
                        if (!string.IsNullOrEmpty(query))
                        {
                            queryable = queryable.Where(l => l.User.FullName != null && l.User.FullName.Contains(query));
                        }
                        break;
                    case "ca":
                        if (!string.IsNullOrEmpty(query))
                        {
                            queryable = queryable.Where(l => l.Shift != null && l.Shift.Equals(query));
                        }
                        break;
                    case "them":
                        queryable = queryable.Where(l => l.IsOverTime != null && l.IsOverTime == true); break;
                    case "trangthai":
                        queryable = queryable.Where(l => l.Status != null && l.Status == false || l.Status == null); break;

                }
            }

            var  PageList =  queryable.OrderBy(l => l.DutyDays).ToPagedList(page, pageSize);
            return await Task.FromResult(View(PageList));
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
                    return RedirectToAction("Index");
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

            return RedirectToAction("Index");
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
            return RedirectToAction("Index");
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
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["NotCreateDay"] = $"Có lỗi xảy ra: {ex.Message}";
                    return RedirectToAction("Index");
                }
            }

            // Nếu model không hợp lệ, trả lại form để sửa
            return View(dutySchedule);
        }
        public async Task<IActionResult>Auto(DateTime? FromDate , DateTime? ToDate)
        {
            var listUser = await _userManager.Users
                .Where(u=>u.Email !="Admin@gmail.com")
                .ToListAsync();
            if (!FromDate.HasValue || !ToDate.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn đầy đủ ngày!";
                return RedirectToAction("CreateDutySchedule");
            }
            if(FromDate < DateTime.Now)
            {
                TempData["FromDateInvalid"] = "Ngày bắt đầu không thể trong quá khứ !";
                return RedirectToAction("CreateDutySchedule");
            }
            if (ToDate < DateTime.Now)
            {
                TempData["ToDateInvalid"] = "Ngày kết thúc không thể trong quá khứ !";
                return RedirectToAction("CreateDutySchedule");
            }
            var ngayBatDau = FromDate.Value;
            var ngayKetThuc = ToDate.Value;

            // Lấy tất cả các ngày từ FromDate đến ToDate, bỏ qua Chủ Nhật
            var danhSachNgayTruc = Enumerable.Range(0, (ngayKetThuc - ngayBatDau).Days + 1)
                .Select(offset => ngayBatDau.AddDays(offset))
                .Where(date => date.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            // Bắt đầu gán ca trực theo danh sách ngày và user
            string[] caTrongNgay = { "Sáng", "Chiều", "Tối" };
            int userIndex = 0;

            foreach (var ngay in danhSachNgayTruc)
            {
                foreach (var ca in caTrongNgay)
                {
                    var user = listUser[userIndex % listUser.Count];

                    _context.DutySchedule.Add(new DutySchedule
                    {
                        UserId = user.Id,
                        DutyDays = ngay,
                        Shift = ca,
                        Status = true,
                        IsOverTime = false,
                        Description = "Tự động"
                    });

                    userIndex++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["DutySuccess"] = "Đã sắp xếp ca trực tự động (không tính Chủ Nhật)";
            return RedirectToAction("Index");
        }
    }
}
