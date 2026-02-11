using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models;

namespace CRM.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Tasks
        public async Task<IActionResult> Index(string filter = "all")
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Notes
                .Include(n => n.Customer)
                .Where(n => n.AuthorId == userId)
                .Where(n => n.ReminderDate != null)
                .Where(n => n.Customer.IsActive); 
            switch (filter.ToLower())
            {
                case "pending":
                    query = query.Where(n => !n.IsReminderDone);
                    break;
                case "completed":
                    query = query.Where(n => n.IsReminderDone);
                    break;
                    // "all" does nothing, returns everything
            }

            var tasks = await query
                .OrderBy(n => n.IsReminderDone) // Pending first
                .ThenBy(n => n.ReminderDate)
                .ToListAsync();

            ViewData["CurrentFilter"] = filter;

            return View(tasks);
        }

        // POST: Toggle Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                var userId = _userManager.GetUserId(User);
                // Security check: ensure user owns the note
                if (note.AuthorId == userId)
                {
                    note.IsReminderDone = !note.IsReminderDone;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
        // [POST] Mark a task as done without reloading the page
        [HttpPost]
        public async Task<IActionResult> MarkDoneAjax(int id)
        {
            var userId = _userManager.GetUserId(User);
            var note = await _context.Notes.FindAsync(id);

            if (note != null && note.AuthorId == userId)
            {
                note.IsReminderDone = true; // Set to Done
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Task not found" });
        }
    }
}