using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Project_Thuc_Tap.Models;

namespace Project_Thuc_Tap.Data
{
    public class DataSeed
    {
        public static async Task DuLieuMau(IServiceProvider dichVu)
        {
            var quanLyNguoiDung = dichVu.GetService<UserManager<User>>();
            var quanLyVaiTro = dichVu.GetService<RoleManager<IdentityRole>>();
            //Thêm một vai trò vào csdl
            await quanLyVaiTro.CreateAsync(new IdentityRole(Controllers.Roles.Roles.Admin.ToString()));
            await quanLyVaiTro.CreateAsync(new IdentityRole(Controllers.Roles.Roles.User.ToString()));
            //Tạo thông tin mặc định cho Admin
            var quanTri = new User
            {
                UserName = "Admin@gmail.com",
                Email = "Admin@gmail.com",
                EmailConfirmed = true,
            };
            var nguoiDungTrongCsdl = await quanLyNguoiDung.FindByEmailAsync(quanTri.Email);
            //Nếu tài khoản Admin không tồn tại 
            if(nguoiDungTrongCsdl is null)
            {
                //Tạo tài khoản Admin mới
                await quanLyNguoiDung.CreateAsync(quanTri, "Admin@123");
                await quanLyNguoiDung.AddToRoleAsync(quanTri, Controllers.Roles.Roles.Admin.ToString());
            }    


        }
    }
}
