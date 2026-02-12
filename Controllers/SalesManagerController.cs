using CRM.DTOS.SalesManager;
using CRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    public class SalesManagerController : Controller
    {
        private readonly ISalesManagerService _salesManagerService;
        private readonly ILogger<SalesManagerController> _logger;

        public SalesManagerController(ISalesManagerService salesManagerService, ILogger<SalesManagerController> logger)
        {
            _salesManagerService = salesManagerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboard = await _salesManagerService.GetDashboardDataAsync();
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading sales manager dashboard: {ex.Message}");
                return View(new SalesManagerDashBoardDto());
            }
        }
    }
}
