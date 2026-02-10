using CRM.DTOS.Admin;
using CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    [Authorize(Roles ="Admin")]

    public class AdminController : Controller
    {
        private readonly IUserAdminService _userAdminService;

        public AdminController(IUserAdminService userAdminService)
        {
            _userAdminService = userAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userAdminService.GetAllUsersAsync();
            return View(users); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> UpdateRole(string id, string newRole)
        {
            var result = await _userAdminService.UpdateUserRoleAsync(id, newRole);
            if (!result) TempData["Error"] = "Update failed.";

            return RedirectToAction(nameof(Index)); 
        }
    }
}
