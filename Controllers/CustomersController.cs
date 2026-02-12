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
            var customers = await _customerService.GetAllCustomersAsync(userId, User.IsInRole("Admin"));
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

            viewModel.Customer.SalesRepId = viewModel.SelectedSalesExecutiveId ?? "";

            await _customerService.CreateAsync(viewModel.Customer);

            return RedirectToAction(nameof(Index));
        }

        // --- 3. DETAILS WORKSPACE ---
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetDetailsAsync(id);
            if (customer == null) return NotFound();

            if (customer.SalesRepId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                return Forbid();

            return View(customer);
        }

        // --- 4. EDIT (Strictly Active Only) ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // SECURITY: If customer is Archived (IsActive == false), return NotFound.
            // This prevents editing profile after deletion.
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (customer == null) return NotFound();

            if (customer.SalesRepId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                return Forbid();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            ModelState.Remove("SalesRepId");

            if (ModelState.IsValid)
            {
                await _customerService.UpdateAsync(id, customer);
                return RedirectToAction(nameof(Details), new { id = customer.Id });
            }
            return View(customer);
        }

        // --- 5. ARCHIVE (Safe Delete) ---
        // Handles the "Archive" button from Edit AND Details pages
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                // Soft Delete
                customer.IsActive = false;
                await _context.SaveChangesAsync();
            }

            // REDIRECT TO INDEX: This ensures you leave the Customer page immediately
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
        public async Task<IActionResult> Archived()
        {
            var userId = _userManager.GetUserId(User);
            var archived = await _customerService.GetAllArchivedAsync(userId, User.IsInRole("Admin"));
            return View(archived);
        }

        public async Task<IActionResult> Restore(int id)
        {
            await _customerService.RestoreAsync(id, _userManager.GetUserId(User), User.IsInRole("Admin"));
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Export()
        {
            var csv = await _customerService.GenerateCsvAsync(_userManager.GetUserId(User));
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "MyCustomers.csv");
        }
    }
}