using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using X.PagedList.Extensions;

namespace Project_Thuc_Tap.Controllers.RoomManager
{
    public class RoomManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RoomManagerController(ApplicationDbContext context) 
        { 
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var listRoom = await _context.Room.ToListAsync();
            var PageList = listRoom.ToPagedList( page, pageSize);
            return View(PageList);
        }
        [HttpGet]
        public IActionResult CreateRooms()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRooms(Room model)
        {
            var name = await _context.Room
                .Select(t => t.RoomName)
                .ToListAsync();
            foreach (var item in name)
            {
                if (item != null)
                {
                    if(model.RoomName == item)
                    {
                        TempData["Name"] = "Phòng đã tồn tại!";
                        return RedirectToAction("CreateRooms");
                    }    
                }    

            }
                try
                {
                    var room = new Room()
                    {
                        RoomName = model.RoomName,
                        Description = model.Description,
                    };
                    _context.Add(room);
                    await _context.SaveChangesAsync();
                    TempData["CreateYes"] = "Đã thêm phòng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["NotCreate"] = "Không thể tạo mới" + ex.Message;
                    return RedirectToAction("Index");
                }
            
        }
        [HttpGet]
        public async Task<IActionResult>EditRooms(int id)
        {
            var Edit = await _context.Room
                .Include(r => r.Department)
                .Where(e=>e.RoomId == id)
                .FirstOrDefaultAsync();
            return View(Edit);
        }
        [HttpPost]
        public async Task<IActionResult> EditRooms(int id, Room model)
        {
            var result = await _context.Room.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            else
            {
                result.RoomName = model.RoomName;
                result.Description = model.Description;
                _context.Update(result);
                await _context.SaveChangesAsync();
                TempData["UpdateRoom"] = "Cập nhật thông tin phòng thành công!";
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> DeleteRooms(int id)
        {
            var delete = await _context.Room.FindAsync(id);
            if (delete == null)
            {
                return NotFound();
            }
            else
            {
                _context.Remove(delete);
                await _context.SaveChangesAsync();
                TempData["DeleteYes"] = $"Đã xoá thành công {delete.RoomName}";
                return RedirectToAction("Index");
            }
        }

    }
}
