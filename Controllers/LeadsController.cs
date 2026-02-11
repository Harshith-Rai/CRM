using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for Dropdowns
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CRM.Controllers
{
    [Authorize]
    public class LeadsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. THE KANBAN BOARD
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var leads = await _context.Leads
                .Include(l => l.Customer) // Bring in the Company Name
                .Where(l => l.SalesRepId == userId)
                .ToListAsync();

            return View(leads);
        }

        // 2. CREATE DEAL FORM
        public IActionResult Create()
        {
            // Get list of Active Customers for the dropdown
            var customers = _context.Customers.Where(c => c.IsActive);
            ViewData["CustomerId"] = new SelectList(customers, "Id", "CompanyName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lead lead)
        {
            // Set the owner and time automatically
            lead.SalesRepId = _userManager.GetUserId(User);
            lead.CreatedAt = DateTime.UtcNow;
            lead.Status = LeadStatus.New; // Always start as New

            // Remove validation for objects we aren't submitting
            ModelState.Remove("Customer");
            ModelState.Remove("SalesRepId");

            if (ModelState.IsValid)
            {
                _context.Add(lead);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If error, reload the dropdown so it doesn't disappear
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CompanyName", lead.CustomerId);
            return View(lead);
        }

        // 3. MOVE CARD (Quick Status Change)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, LeadStatus status)
        {
            var lead = await _context.Leads.FindAsync(id);
            if (lead != null)
            {
                lead.Status = status;

                // If the deal is finished, mark the date
                if (status == LeadStatus.Won || status == LeadStatus.Lost)
                {
                    lead.ClosedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. DELETE LEAD
        public async Task<IActionResult> Delete(int id)
        {
            var lead = await _context.Leads.FindAsync(id);
            if (lead != null)
            {
                _context.Leads.Remove(lead); // Hard delete for leads is usually okay, or use Soft Delete if you prefer
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}