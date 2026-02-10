using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models;

namespace CRM.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Tasks
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var today = DateTime.UtcNow.Date;

            // Fetch Notes that have a Reminder Date set for Today or Future
            var tasks = await _context.Notes
                .Include(n => n.Customer) // So we show Company Name
                .Where(n => n.AuthorId == userId) // Only my tasks
                .Where(n => n.ReminderDate != null) // Must have a date
                .Where(n => n.ReminderDate >= today) // Don't show old stuff
                .OrderBy(n => n.ReminderDate) // Show soonest first
                .ToListAsync();

            return View(tasks);
        }
    }
}