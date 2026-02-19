using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CRM.Models;
using CRM.Services;
namespace CRM.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INavigation _navigation;

        public HomeController(UserManager<ApplicationUser> userManager,INavigation navigation)
        {
            _userManager = userManager;
            _navigation = navigation;
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(_navigation.GetDashboardUrl(User));
            }
            return View();
        }
    }
}
