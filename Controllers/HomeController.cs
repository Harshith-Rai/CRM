using CRM.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;
namespace CRM.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if(User.IsInRole("Admin"))
                    return RedirectToAction("dashboard", "Admin");
                else 
                    return RedirectToAction("dashboard", "SalesManager");
            }
            return View();
        }
    }
}
