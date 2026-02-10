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
        public IActionResult Create()
        {
            return View(new Customer());
        }
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

            // --- FIX: FORCE UTC FOR POSTGRESQL ---
            if (reminderDate.HasValue)
            {
                // Convert the date from the form to UTC so Postgres accepts it
                reminderDate = DateTime.SpecifyKind(reminderDate.Value, DateTimeKind.Utc);
            }

            var note = new Note
            {
                CustomerId = customerId,
                Title = title ?? "Interaction",
                Content = content,
                CreatedAt = DateTime.UtcNow, // This is already UTC, so it's safe
                ReminderDate = reminderDate, // Now this is safe too
                AuthorId = _userManager.GetUserId(User)
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = customerId });
        }
        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            // Remove validation for SalesRepId as we aren't changing it in the form usually
            ModelState.Remove("SalesRepId");

            if (ModelState.IsValid)
            {
                try
                {
                    // Keep the original creation date and owner
                    var existingCustomer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingCustomer != null)
                    {
                        customer.CreatedAt = existingCustomer.CreatedAt;
                        customer.SalesRepId = existingCustomer.SalesRepId;
                        customer.IsActive = existingCustomer.IsActive;
                    }

                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Customers.Any(e => e.Id == customer.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
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

        // --- 6. ARCHIVE / RECYCLE BIN ---

        // GET: Customers/Archived
        public async Task<IActionResult> Archived()
        {
            var userId = _userManager.GetUserId(User);

            // Fetch ONLY the inactive customers for this user
            var archivedCustomers = await _context.Customers
                .Where(c => !c.IsActive) // Notice the "!" (Not Active)
                .Where(c => c.SalesRepId == userId || User.IsInRole("Admin")) // Security check
                .ToListAsync();

            return View(archivedCustomers);
        }

        // GET: Customers/Restore/5
        public async Task<IActionResult> Restore(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            // Security check: Ensure they own the record or are Admin
            if (customer != null && (customer.SalesRepId == userId || User.IsInRole("Admin")))
            {
                customer.IsActive = true; // <--- THE MAGIC SWITCH
                await _context.SaveChangesAsync();
            }

            // Send them back to the main list to see their restored friend
            return RedirectToAction(nameof(Index));
        }
    }
}