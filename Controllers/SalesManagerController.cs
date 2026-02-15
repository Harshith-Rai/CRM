using CRM.DTOS.SalesManager;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    //[Authorize]
    [Route("SalesManager/dashboard")]
    public class SalesManagerController : Controller
    {
        private readonly ISalesManagerService _salesManagerService;
        private readonly ILogger<SalesManagerController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesManagerController(ISalesManagerService salesManagerService, ILogger<SalesManagerController> logger,UserManager<ApplicationUser> user)
        {
            _salesManagerService = salesManagerService;
            _logger = logger;
            _userManager = user;
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
                var dashboard = await _salesManagerService.GetDashboardDataAsync(userId,isManager);
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading sales manager dashboard: {ex.Message}");
                return View(new DashboardBaseDto());
            }
        }
    }
}
