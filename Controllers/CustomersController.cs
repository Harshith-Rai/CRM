using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CRM.Models;
using CRM.Services;
using System.Text;

namespace CRM.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(ICustomerService customerService, UserManager<ApplicationUser> userManager)
        {
            _customerService = customerService;
            _userManager = userManager;
        }

        // --- 1. LIST VIEW ---
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Call the new method that returns BOTH Active and Inactive
            var customers = await _customerService.GetAllCustomersAsync(userId, User.IsInRole("Admin"));

            return View(customers);
        }

        // --- 2. CREATE ---
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Customer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            ModelState.Remove("SalesRepId");
            if (ModelState.IsValid)
            {
                await _customerService.CreateAsync(customer, _userManager.GetUserId(User));
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // --- 3. DETAILS WORKSPACE ---
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetDetailsAsync(id);
            if (customer == null) return NotFound();

            // Security Check
            if (customer.SalesRepId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                return Forbid();

            return View(customer);
        }

        // --- 4. EDIT ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetDetailsAsync(id);
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
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // --- 5. NOTES & INTERACTIONS ---
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

        // --- 6. SOFT DELETE ---
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.SoftDeleteAsync(id, _userManager.GetUserId(User), User.IsInRole("Admin"));
            return RedirectToAction(nameof(Index));
        }

        // --- 7. ARCHIVE & RESTORE ---
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

        // --- 8. EXPORT ---
        public async Task<IActionResult> Export()
        {
            var csv = await _customerService.GenerateCsvAsync(_userManager.GetUserId(User));
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "MyCustomers.csv");
        }
    }
}