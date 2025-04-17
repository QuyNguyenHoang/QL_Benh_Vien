using Microsoft.AspNetCore.Mvc;

namespace Project_Thuc_Tap.Controllers
{
    public class DepartmentManager : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
