﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Project_Thuc_Tap.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using Project_Thuc_Tap.Controllers.Roles;
using Microsoft.AspNetCore.Hosting;
using X.PagedList;
using X.PagedList.Extensions;

namespace Project_Thuc_Tap.Controllers.UserManager
{
    public class UserManagerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagerController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> User_ViewMain(int page = 1)
        {
            int pageSize = 5;
            var users = await _userManager.Users.OrderBy(u => u.Id).ToListAsync();
            var pagedUsers = users.ToPagedList(page, pageSize);
            return View(pagedUsers);
        }
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var listRoom = await _context.Room.ToListAsync();
            ViewBag.Room = listRoom;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Khởi tạo đối tượng User (ApplicationUser) mới
            var user = new User
            {
                FullName = model.FullName,
                Sex = model.Sex,
                Address = model.Address,
                BirthDate = model.BirthDate,
                Email = model.Email,
                UserName = model.Email, // Nên dùng email làm username
                PhoneNumber = model.PhoneNumber,
                CreatedDate = DateTime.Now
            };

            // Tạo user với mật khẩu mặc định
            var result = await _userManager.CreateAsync(user, "1111111");

            if (result.Succeeded)
            {
                // Gán role nếu có
                await _userManager.AddToRoleAsync(user, "User");
                return RedirectToAction("Index");
            }

            // Hiển thị lỗi nếu tạo user thất bại
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }





        // GET: Hiển thị thông tin User dựa trên ID
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy ID người dùng!");
            }

            // Tìm User dựa trên ID
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng!");
            }

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(User model, IFormFile? Picture)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Lấy user hiện tại từ DB
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound("Không tìm thấy người dùng!");

                // Cập nhật thông tin text
                user.FullName = model.FullName;
                user.Sex = model.Sex;
                user.Address = model.Address;
                user.BirthDate = model.BirthDate;
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;


                // Xử lý ảnh đại diện mới
                if (Picture != null && Picture.Length > 0)
                {
                    // Lấy tên file gốc từ ảnh mới
                    var fileName = Path.GetFileName(Picture.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);

                    // Kiểm tra xem file có tồn tại chưa
                    if (!System.IO.File.Exists(filePath))
                    {
                        // Nếu file chưa có -> lưu file mới vào thư mục
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Picture.CopyToAsync(stream);
                        }
                    }

                    // Cập nhật tên ảnh vào User (dù ảnh có cũ hay mới thì cũng gán lại)
                    user.Picture = fileName;
                }


                // Lưu thay đổi vào Identity
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction("User_ViewMain");
                }
                else
                {
                    TempData["Error"] = "Có lỗi khi cập nhật người dùng: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra khi cập nhật người dùng: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy ID người dùng!");
            }

            // Tìm user theo ID
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng!");
            }

            // Lấy vai trò "Admin" từ bảng AspNetRoles
            var adminRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            // Kiểm tra trong bảng AspNetUserRoles
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Chặn xóa nếu user là Admin
            if (isAdmin && adminRole != null)
            {
                TempData["CanNotDelete"] = $"Người dùng '{user.FullName}' là Admin, không thể xóa!";
                return RedirectToAction("User_ViewMain");
            }


            try
            {
                // Xóa ảnh đại diện nếu có
                if (!string.IsNullOrEmpty(user.Picture))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", user.Picture);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Xóa người dùng
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["DeleteSuccess"] = $"Đã xóa người dùng '{user.FullName}' thành công!";
                    return RedirectToAction("User_ViewMain");
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng!";
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa: {ex.Message}";
                return View(user);
            }
        }

        [HttpGet("/search")]
        public async Task<IActionResult> Search(DateTime? date, string? filterType, string query, int  page =1)
        {
            int pageSize = 5;
            var users = _userManager.Users;


            if (date.HasValue)
            {
                users = users.Where(nv => nv.CreatedDate.Date == date.Value.Date || (nv.BirthDate.HasValue && nv.BirthDate.Value.Date == date.Value.Date));
            }
            if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(query))
            {
                switch (filterType.ToLower())
                {
                    case "ten":
                        users = users.Where(nv => nv.FullName != null && nv.FullName.Contains(query));
                        break;
                    case "phong":
                        users = users.Where(nv => nv.RoomId.HasValue && nv.RoomId.Value.ToString().Contains(query));
                        break;
                    case "khoa":
                        users = users.Where(nv => nv.RoomId.HasValue && nv.RoomId.Value.ToString().Contains(query));
                        break;
                    case "sodienthoai":
                        users = users.Where(nv => nv.PhoneNumber !=null && nv.PhoneNumber.Contains(query));
                        break;
                        
                }
            }
            var count = users.Count();
            ViewBag.Count = count;
            ViewBag.Query = query;
            var searchResults = await users
                .OrderBy(nv => nv.Id)
                .ToListAsync();
            var PageList = searchResults.ToPagedList(page, pageSize);
            return View("Search", PageList);
        }


    }

}


