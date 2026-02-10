using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Needed for database access
using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Controllers
{
    [Authorize] // Lock the dashboard so only logged-in users see it
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var startDate = DateTime.SpecifyKind(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                DateTimeKind.Utc);
            var model = new DashboardViewModel
            {
                // Count Active Customers
                TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive),

                // Count New Customers since the 1st of the month
                NewCustomersThisMonth = await _context.Customers
                    .CountAsync(c => c.IsActive && c.CreatedAt >= startDate),

                // Count Total People
                TotalContacts = await _context.Contacts.CountAsync(),

                // Get the 5 most recent notes (Interaction Feed)
                RecentActivities = await _context.Notes
                    .Include(n => n.Customer) // Grab company name too
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                // Group by Industry for the chart
                CustomersByIndustry = await _context.Customers
                    .Where(c => c.IsActive && c.Industry != null)
                    .GroupBy(c => c.Industry)
                    .Select(g => new { Industry = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Industry, x => x.Count)
            };

            return View(model);
        }
    }
}