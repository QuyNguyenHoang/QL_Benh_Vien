// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Project_Thuc_Tap.Models;
using Project_Thuc_Tap.Controllers.Roles;
using Microsoft.AspNetCore.Hosting;

namespace Project_Thuc_Tap.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        //Bỏ Idenitity 
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegisterModel(
             UserManager<User> userManager,
             IUserStore<User> userStore,
             SignInManager<User> signInManager,
              IWebHostEnvironment webHostEnvironment,
             ILogger<RegisterModel> logger,
             IEmailSender emailSender)
            

        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _emailSender = emailSender;
            



        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự, trong đó có ít nhất 1 ký tự in hoa(Z), 1 ký tự đặc biệt(@), chữ và số", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không đúng!")]
            public string ConfirmPassword { get; set; }

            //======================================================
            // Các thuộc tính tùy chỉnh
            [Required(ErrorMessage ="Vui lòng nhập đầy đủ họ và tên!")]
            [Display(Name = "Full Name")]
           
            public string FullName { get; set; }

            [Required(ErrorMessage ="Vui lòng chọn giới tính!")]
            [Display(Name = "Sex")]
            public bool Sex { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }

            [Required(ErrorMessage ="Vui lòng nhập ngày tháng năm sinh!")]
            [Display(Name = "Birth Date")]
            [DataType(DataType.Date)]
            public DateTime? BirthDate { get; set; }

            [Display(Name = "Picture")]
            [BindProperty]
            public IFormFile Picture { get; set; }

            [Display(Name = "PhoneNumber")]
            public string PhoneNumber { get; set; }



        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync( string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                //=============
                // Thiết lập các thuộc tính tùy chỉnh
                user.FullName = Input.FullName;
                user.Sex = Input.Sex;
                user.Address = Input.Address;
                user.BirthDate = Input.BirthDate;
                user.PhoneNumber = Input.PhoneNumber;

                if(Input.Picture != null && Input.Picture.Length > 0)
{
                    try
                    {
                        var fileExtension = Path.GetExtension(Input.Picture.FileName).ToLower();
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Input.Picture", "Chỉ hỗ trợ định dạng ảnh JPG, JPEG, PNG, GIF.");
                            return Page();
                        }

                        // Đặt tên file tránh trùng lặp
                        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                        // Tạo đường dẫn đầy đủ
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", uniqueFileName);

                        // Lưu file vào thư mục
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Input.Picture.CopyToAsync(stream);
                        }

                        // Lưu tên file vào DB (chỉ cần lưu tên, không lưu cả đường dẫn)
                        user.Picture = uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Input.Picture", $"Lỗi khi tải ảnh lên: {ex.Message}");
                        return Page();
                    }
                }









                // Tiếp tục tạo tài khoản người dùng
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Thêm phân quyền
                    await _userManager.AddToRoleAsync(user, Controllers.Roles.Roles.User.ToString());
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Xác nhận tài khoản",
                        $"Hãy xác nhận tài khoản bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        TempData["Message"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
                        return RedirectToPage("Login");
                    }
                    else
                    {
                        TempData["Message"] = "Đăng ký thành công! Hãy đăng nhập ngay.";
                        return RedirectToPage("Login");
                    }
                }

                // Nếu có lỗi, thêm thông báo lỗi
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Nếu có lỗi, hiển thị lại form
            return Page();
        }


        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        
        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
