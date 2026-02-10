using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomersController(ICustomerService customerService, UserManager<ApplicationUser> userManager)
    {
        _customerService = customerService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var customers = await _customerService.GetAllActiveAsync(userId, User.IsInRole("Admin"));
        return View(customers);
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

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _customerService.GetDetailsAsync(id);
        if (customer == null) return NotFound();

        if (customer.SalesRepId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            return Forbid();

        return View(customer);
    }

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

    public async Task<IActionResult> Delete(int id)
    {
        await _customerService.SoftDeleteAsync(id, _userManager.GetUserId(User), User.IsInRole("Admin"));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export()
    {
        var csv = await _customerService.GenerateCsvAsync(_userManager.GetUserId(User));
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "MyCustomers.csv");
    }

    // ... Repeat pattern for Edit, Restore, and Archived
}