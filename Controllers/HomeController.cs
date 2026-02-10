using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IHomeService _homeService; // Only ask for the Service
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(IHomeService homeService, UserManager<ApplicationUser> userManager)
    {
        _homeService = homeService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        // One line of code to get everything!
        var model = await _homeService.GetDashboardDataAsync(userId);

        return View(model);
    }
}