using CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    [Authorize(Roles ="admin")]
    public class AdminUserController : Controller
    {
        private readonly IUserAdminService _adminService;

        public AdminUserController(IUserAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users); // Looks for Views/UserAdmin/Index.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string id, string newRole)
        {
            var result = await _adminService.UpdateUserRoleAsync(id, newRole);
            if (!result) TempData["Error"] = "Update failed.";

            return RedirectToAction(nameof(Index)); // Reloads the list
        }
    }
}
