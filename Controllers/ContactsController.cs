using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers
{
    public class ContactsController : Controller
    {
        private readonly AppDbContext _context;

        public ContactsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Contacts/Create?customerId=5
        public IActionResult Create(int? customerId) // Changed to nullable int? for safety
        {
            if (customerId == null || customerId == 0)
            {
                return BadRequest("A Customer ID is required to add a contact.");
            }

            // Create the contact and PRE-FILL the ID so the View knows who it belongs to
            var contact = new Contact
            {
                CustomerId = customerId.Value
            };

            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact contact)
        {
            // CRITICAL FIX: The "Customer" object is null because we only sent the ID.
            // We tell ASP.NET to ignore validating the parent object.
            ModelState.Remove("Customer");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Contacts.Add(contact);
                    await _context.SaveChangesAsync();

                    // Success! Go back to the Company's page
                    return RedirectToAction("Details", "Customers", new { id = contact.CustomerId });
                }
                catch (Exception ex)
                {
                    // If database fails, show the error on the form
                    ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
                }
            }

            // If we got here, something is wrong. Reload the form so user can fix it.
            return View(contact);
        }
    }
}