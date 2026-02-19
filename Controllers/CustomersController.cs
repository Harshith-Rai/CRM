using CRM.Data;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
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
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10,string searchTerm=null,String status=null,String industry=null)
        {
            var userId = _userManager.GetUserId(User);

            // FIX 1: Allow "Sales Manager" to see all records too
            bool canSeeAll = User.IsInRole("Admin") || User.IsInRole("SalesManager");

            var customers = await _customerService.GetAllCustomersAsync(userId, canSeeAll, pageNumber, pageSize,searchTerm,status,industry);
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
            var currentUserId = _userManager.GetUserId(User);
            bool isManagerialRole = User.IsInRole("Admin") || User.IsInRole("Sales Manager");

            if (isManagerialRole)
            {
                viewModel.Customer.SalesRepId = viewModel.SelectedSalesExecutiveId ?? currentUserId;
            }
            else
            {
                viewModel.Customer.SalesRepId = currentUserId;
            }

           ModelState.Remove("SalesExecutives");
           ModelState.Remove("SelectedSalesExecutiveId");

            // 3. Remove Customer navigation properties
            ModelState.Remove("Customer.SalesRepId"); // We set this manually above
            ModelState.Remove("Customer.SalesRep");
            ModelState.Remove("Customer.Contacts");
            ModelState.Remove("Customer.Notes");
            // --- FIX END ---

            if (ModelState.IsValid)
            {
                await _customerService.CreateAsync(viewModel.Customer);
                return RedirectToAction(nameof(Index));
            }

            viewModel.SalesExecutives = await _customerService.GetSalesExecutivesAsync();
            return View(viewModel);
        }
        // --- 3. DETAILS WORKSPACE ---
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetDetailsAsync(id);
            if (customer == null) return NotFound();

            // FIX 3: Allow "Sales Manager" to view details
            bool hasAccess = customer.SalesRepId == _userManager.GetUserId(User) ||
                             User.IsInRole("Admin") ||
                             User.IsInRole("SalesManager");

            if (!hasAccess) return Forbid();

            return View(customer);
        }

        // --- 4. EDIT ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (customer == null) return NotFound();

            // FIX 4: Allow "Sales Manager" to edit
            bool hasAccess = customer.SalesRepId == _userManager.GetUserId(User) ||
                             User.IsInRole("Admin") ||
                             User.IsInRole("SalesManager");

            if (!hasAccess) return Forbid(); 

            if (User.IsInRole("Admin")|| User.IsInRole("SalesManager"))
            {
                var salesExecutives = await _userManager.GetUsersInRoleAsync("SalesExecutive");
                ViewBag.SalesExecutives = salesExecutives.OrderBy(u => u.FullName).ToList();
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {

            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {

                await _customerService.UpdateAsync(id, customer);
                return RedirectToAction("index","Customers");
            }
            return View(customer);
        }

        // --- 5. ARCHIVE (Safe Delete) ---
        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerService.InactivateCustomerAsync(id);

            return RedirectToAction("index","Customers");
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
                customer.IsHiddenFromBin = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Archived));
        }
    }
}   