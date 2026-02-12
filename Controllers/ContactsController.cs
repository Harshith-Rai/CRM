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
        public IActionResult Create(int? customerId)
        {
            if (customerId == null || customerId == 0)
            {
                return BadRequest("A Customer ID is required to add a contact.");
            }

            var contact = new Contact
            {
                CustomerId = customerId.Value
            };

            return View(contact);
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact contact)
        {
            ModelState.Remove("Customer"); // Ignore parent validation

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Contacts.Add(contact);
                    await _context.SaveChangesAsync();

                    // IMPORTANT: Ensure "Customers" matches your Controller filename (e.g. CustomersController.cs)
                    // If your file is CustomerController.cs, change this string to "Customer"
                    return RedirectToAction("Details", "Customers", new { id = contact.CustomerId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
                }
            }

            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);

            // --- SAFETY CHECK ---
            // If contact is already gone, don't show an error. Just go back to the main list.
            if (contact == null)
            {
                // Ensure this matches your main customer list controller name
                return RedirectToAction("Index", "Customers");
            }

            var customerId = contact.CustomerId; // Save ID to return to the right page

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            // IMPORTANT: Redirect back to the Company page
            // Change "Customers" to "Customer" if your controller is singular
            return RedirectToAction("Details", "Customers", new { id = customerId });
        }
    }
}