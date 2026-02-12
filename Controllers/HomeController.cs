//using System.Diagnostics;
//using CRM.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace CRM.Controllers
//{
//    public class HomeController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}

using System.Diagnostics;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    public class HomeController : Controller
    {
        // 1. This serves your new animated Landing Page
        public IActionResult Index()
        {
            return View();
        }

        // 2. Add this: The page users see AFTER logging in
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
