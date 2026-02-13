using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
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
            ViewBag.Customers = new SelectList(_context.Customers.Where(c => c.IsActive), "Id", "CompanyName");

            if (User.IsInRole("Admin"))
            {
                var salesReps = await _userManager.GetUsersInRoleAsync("Sales Rep");
                ViewBag.SalesReps = new SelectList(salesReps, "Id", "FullName");
            }
            

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
            }

            var tasks = await query
                .OrderBy(n => n.IsReminderDone)
                .ThenBy(n => n.ReminderDate)
                .ToListAsync();

            ViewData["CurrentFilter"] = filter;

            return View(tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int customerId, string title, string content, DateTime? reminderDate, string? assignedToId)
        {
            var currentUserId = _userManager.GetUserId(User);
            string targetOwnerId = currentUserId;

            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(assignedToId))
            {
                targetOwnerId = assignedToId;
            }

            if (ModelState.IsValid)
            {
                var note = new Note
                {
                    CustomerId = customerId,
                    Title = title,
                    Content = content,
                    ReminderDate = reminderDate,
                    IsReminderDone = false,
                    CreatedAt = DateTime.UtcNow,
                    AuthorId = targetOwnerId 
                };

                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                var userId = _userManager.GetUserId(User);
                if (note.AuthorId == userId)
                {
                    note.IsReminderDone = !note.IsReminderDone;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkDoneAjax(int id)
        {
            var userId = _userManager.GetUserId(User);
            var note = await _context.Notes.FindAsync(id);

            if (note != null && note.AuthorId == userId)
            {
                note.IsReminderDone = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Task not found" });
        }
    }
}