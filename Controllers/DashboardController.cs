using CRM.Data;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers 
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDashBoardService _dashboardService;

        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager,IDashBoardService dashboardServivce)
        {
            _context = context;
            _userManager = userManager;
            _dashboardService = dashboardServivce;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                bool isManager = true;
                string userId = null;
                if (User.IsInRole("SalesExecutive"))
                {
                    isManager = false;
                    userId = _userManager.GetUserId(User);
                }
                var dashboard = await _dashboardService.GetDashboardDataAsync(userId, isManager);
                return View(dashboard);
            }
            catch (Exception ex)
            {
                return View(new DashboardBaseDto());
            }
        }
    }
}