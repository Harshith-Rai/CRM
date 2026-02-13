using Microsoft.AspNetCore.Mvc;
using CRM.Services;
using CRM.Models;

namespace CRM.Controllers;

public class NotesController : Controller
{
    private readonly ICustomerService _customerService;

    public NotesController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // GET: Notes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var note = await _customerService.GetNoteAsync(id);
        if (note == null) return NotFound();
        return View(note);
    }

    // POST: Notes/Edit/5
    // POST: Notes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Note note)
    {
        ModelState.Remove("Customer");
        ModelState.Remove("Author");

        if (ModelState.IsValid)
        {
            await _customerService.UpdateNoteAsync(note);
            return RedirectToAction("Details", "Customers", new { id = note.CustomerId });
        }

        return View(note);
    }

    // GET: Notes/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _customerService.GetNoteAsync(id);
        if (note != null)
        {
            int customerId = note.CustomerId;
            await _customerService.DeleteNoteAsync(id);
            return RedirectToAction("Details", "Customers", new { id = customerId });
        }
        return RedirectToAction("Index", "Customers");
    }
}