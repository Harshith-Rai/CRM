using CRM.DTOS;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService; // Only ask for the Service
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _userManager = userManager;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);

        // One line of code to get everything!
        var model = await _adminService.GetDashboardDataAsync(userId);

        return View(model);
    }

    [HttpGet("users")]
    public async Task<IActionResult> Index()
    {
        var users = await _adminService.GetAllUsersAsync();
        return View(users); // Ensure file is at Views/Admin/Index.cshtml
    }

    // URL: /admin/update-role
    [HttpPost("update-role")]
    public async Task<IActionResult> UpdateRole(string id, string newRole)
    {
        var result = await _adminService.UpdateUserRoleAsync(id, newRole);
        if (!result) TempData["Error"] = "Update failed.";

        // Use the explicit route to avoid confusion
        return RedirectToAction(nameof(Index));
    }

    [HttpDelete("/Admin/delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var result = await _adminService.DeleteUserAsync(id);
            if (!result) return Json(new { success = false, message = "Error deleting user." });

            return Json(new { success = true, message = "User deleted successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("create-user")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AddUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields correctly.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _adminService.RegisterNewUser(model);

        if (result.Succeeded)
        {
            TempData["Success"] = "User created successfully!";
        }
        else
        {
            // This captures "Password must have a digit", "Password too short", etc.
            TempData["Error"] = string.Join("\\n", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }
}