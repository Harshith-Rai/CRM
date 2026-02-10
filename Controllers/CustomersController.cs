using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models;
using System.Security.Claims;

namespace CRM.Controllers
{
    [Authorize] // 1. Locks this entire controller. You MUST be logged in.
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- 1. LIST VIEW (The Rolodex) ---
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Filter: Show only Active customers.
            var query = _context.Customers.Where(c => c.IsActive);

            // RBAC: If not Admin, filter by the logged-in user's ID.
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(c => c.SalesRepId == userId);
            }

            return View(await query.ToListAsync());
        }

        // --- 2. CREATE (GET & POST) ---
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            // Ignore validation for SalesRepId (we set it manually below)
            ModelState.Remove("SalesRepId");

            if (ModelState.IsValid)
            {
                // Auto-assign owner to current user
                customer.SalesRepId = _userManager.GetUserId(User);
                customer.IsActive = true;
                customer.CreatedAt = DateTime.UtcNow;

                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // --- 3. DETAILS WORKSPACE (Rolodex + Memory) ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            // Eager Load Contacts and Notes so they appear on the page
            var customer = await _context.Customers
                .Include(c => c.Contacts)
                .Include(c => c.Notes)
                .ThenInclude(n => n.Author) // Get the name of the note writer
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            // Security Check: Prevent accessing others' data via URL
            if (customer.SalesRepId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(customer);
        }

        // --- 4. ADD NOTE (The Memory) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int customerId, string title, string content, DateTime? reminderDate)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", new { id = customerId });

            var note = new Note
            {
                CustomerId = customerId,
                Title = title ?? "Interaction",
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ReminderDate = reminderDate,
                AuthorId = _userManager.GetUserId(User)
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = customerId });
        }

        // --- 5. SOFT DELETE ---
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            // Only Owner or Admin can delete
            if (customer != null && (customer.SalesRepId == userId || User.IsInRole("Admin")))
            {
                customer.IsActive = false; // Soft Delete
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}