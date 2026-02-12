using CRM.Data;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CRM.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public CustomersController(ICustomerService customerService, UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _customerService = customerService;
            _userManager = userManager;
            _context = context;
        }

        // --- 1. LIST VIEW ---
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // FIX 1: Allow "Sales Manager" to see all records too
            bool canSeeAll = User.IsInRole("Admin") || User.IsInRole("Sales Manager");

            var customers = await _customerService.GetAllCustomersAsync(userId, canSeeAll);
            return View(customers);
        }

        // --- 2. CREATE ---
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var salesExecutives = await _customerService.GetSalesExecutivesAsync();
            var viewModel = new CreateCustomerViewModel
            {
                Customer = new Customer(),
                SalesExecutives = salesExecutives
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerViewModel viewModel)
        {
            // FIX 2: If the user is a Sales Rep, Force assign the ID to themselves
            // This prevents errors if the dropdown was hidden in the UI
            if (User.IsInRole("Sales Rep"))
            {
                viewModel.Customer.SalesRepId = _userManager.GetUserId(User);
            }
            else
            {
                viewModel.Customer.SalesRepId = viewModel.SelectedSalesExecutiveId ?? _userManager.GetUserId(User);
            }

            await _customerService.CreateAsync(viewModel.Customer);
            return RedirectToAction(nameof(Index));
        }

        // --- 3. DETAILS WORKSPACE ---
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetDetailsAsync(id);
            if (customer == null) return NotFound();

            // FIX 3: Allow "Sales Manager" to view details
            bool hasAccess = customer.SalesRepId == _userManager.GetUserId(User) ||
                             User.IsInRole("Admin") ||
                             User.IsInRole("Sales Manager");

            if (!hasAccess) return Forbid();

            return View(customer);
        }

        // --- 4. EDIT ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.SalesRep)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (customer == null) return NotFound();

            // Check access: Owner, Admin, or Sales Manager
            bool hasAccess = customer.SalesRepId == _userManager.GetUserId(User) ||
                             User.IsInRole("Admin") || User.IsInRole("Sales Manager");
            if (!hasAccess) return Forbid();

            // Only Admins need the dropdown list
            if (User.IsInRole("Admin"))
            {
                var salesExecutives = await _userManager.GetUsersInRoleAsync("SalesExecutive");
                ViewBag.SalesExecutives = salesExecutives.OrderBy(u => u.FullName).ToList();
            }

            return View(customer);
        }

        // CustomersController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            ModelState.Remove("SalesRepId"); // Prevents validation errors for read-only fields

            if (ModelState.IsValid)
            {
                await _customerService.UpdateAsync(id, customer);
                return RedirectToAction(nameof(Details), new { id = customer.Id });
            }

            // Re-populate list to prevent 'Value cannot be null' error on reload
            if (User.IsInRole("Admin"))
            {
                var salesExecutives = await _userManager.GetUsersInRoleAsync("SalesExecutive");
                ViewBag.SalesExecutives = salesExecutives.OrderBy(u => u.FullName).ToList();
            }

            return View(customer);
        }

        // --- 5. ARCHIVE (Safe Delete) ---
        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Ensure only Admins can delete/archive
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                // 1. Soft Delete: Mark as Inactive
                customer.IsActive = false;

                // 2. Set the Archive Time (CRITICAL STEP)
                // Use UtcNow to prevent PostgreSQL errors
                customer.ArchivedAt = DateTime.UtcNow;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        // --- 6. NOTES ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int customerId, string title, string content, DateTime? reminderDate)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                await _customerService.AddNoteAsync(customerId, title, content, reminderDate, _userManager.GetUserId(User));
            }
            return RedirectToAction("Details", new { id = customerId });
        }

        // --- 7. UTILITIES ---

        // FIX 6: Restrict Access to Archived List
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archived()
        {
            var userId = _userManager.GetUserId(User);
            var archived = await _customerService.GetAllArchivedAsync(userId, User.IsInRole("Admin"));
            return View(archived);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            await _customerService.RestoreAsync(id, _userManager.GetUserId(User), User.IsInRole("Admin"));
            return RedirectToAction(nameof(Index));
        }

        // FIX 7: Prevent Sales Reps from Exporting
        [Authorize(Roles = "Admin, Sales Manager")]
        public async Task<IActionResult> Export()
        {
            var csv = await _customerService.GenerateCsvAsync(_userManager.GetUserId(User));
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "MyCustomers.csv");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Hide(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // Mark as hidden so it disappears from the list
                customer.IsHiddenFromBin = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Archived));
        }
    }
}   