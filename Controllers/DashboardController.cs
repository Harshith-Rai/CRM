using CRM.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers // Namespace is important!
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive), // Only count Active ones!
                TotalContacts = await _context.Contacts.CountAsync(),

          
                RecentNotes = await _context.Notes
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .Select(n => new
                    {
                        n.Id,
                        n.Title,
                        n.Content,
                        CreatedAt = n.CreatedAt,
                        CustomerName = n.Customer.CompanyName // Flatten the data for easier use
                    })
                    .ToListAsync()
            };

            return Ok(stats);
        }
    }
}