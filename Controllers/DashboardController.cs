using CRM.Data;
using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    public DashboardController(AppDbContext context) { _context = context; }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = new
        {
            TotalCustomers = await _context.Customers.CountAsync(),
            TotalContacts = await _context.Contacts.CountAsync(),
            RecentNotes = await _context.Notes.OrderByDescending(n => n.CreatedAt).Take(5).ToListAsync()
        };
        return Ok(stats);
    }
}
