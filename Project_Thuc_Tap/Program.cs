using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Data;
using Project_Thuc_Tap.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using OfficeOpenXml;
using NuGet.Packaging;



var builder = WebApplication.CreateBuilder(args);

//Cấu hình excel






// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddControllersWithViews();

// Đăng ký DbContext và Identity


builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IEmailSender, FakeEmailSender>();

var app = builder.Build();
//===============================================================
//Thêm để kết nối với điện thoại
//builder.Services.AddControllersWithViews();
//app.UseRouting();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//// Mở IP và port để nhận kết nối từ thiết bị khác trong mạng
//app.Urls.Add("http://0.0.0.0:5000");

//app.Run();
//===================================================================

//Code liên quan tới phân quyền
//Sau khi chay lan dau comment lai
//using (var scope = app.Services.CreateScope())
//{
//    await DataSeed.DuLieuMau(scope.ServiceProvider);
//}
//=====================================================
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
public class FakeEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"📧 Gửi email đến: {email}, Chủ đề: {subject}");
        return Task.CompletedTask; // Giả lập hoàn tất mà không lỗi
    }
}