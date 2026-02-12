using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            // Populate a ViewModel here with counts/metrics if needed.
            return View();
        }
    }
}