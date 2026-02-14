using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models;
using CRM.Services;

namespace CRM.Controllers
{
    [Authorize]
    [Route("tasks")]
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITaskService taskService;

        public TasksController(AppDbContext context, UserManager<ApplicationUser> userManager,ITaskService taskService)
        {
            _context = context;
            _userManager = userManager;
            this.taskService = taskService;
        }

        // GET: /Tasks
        public async Task<IActionResult> Index(string filter = "All")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userId = user.Id;
            // Get the first role or default to empty string
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";

            // 1. Get the raw List of users
            var users = await taskService.GetAssignableUsers(userId, role);

            var tasks = await taskService.GetAllTasks(userId);

            if (filter == "pending") tasks = tasks.Where(t => t.Status == false).ToList();
            if (filter == "completed") tasks = tasks.Where(t => t.Status == true).ToList();

            var taskViewModel = new TaskDashBoardViewModel
            {
                Tasks = tasks,

                // 2. Map the List to a SelectList (resolves the red squiggles)
                // "Id" is the value sent to the server, "FullName" is what the user sees
                AssigneeList = new SelectList(users, "Id", "FullName"),

                CurrentFilter = filter
            };

            return View(taskViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Activity model)
        {
            // Fix for PostgreSQL: Convert the date to UTC
            // PostgreSQL throws an error if Kind is 'Unspecified'
            model.DueDate = DateTime.SpecifyKind(model.DueDate, DateTimeKind.Utc);

            if (string.IsNullOrEmpty(model.AssignTo))
            {
                model.AssignTo = _userManager.GetUserId(User);
            }

                await taskService.CreateTask(model);
                return RedirectToAction(nameof(Index));

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // This explicitly maps the URL /Tasks/UpdateStatus/{id}/{status}
        [Route("Tasks/UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var result = await taskService.UpdateStatus(id,true);
            if (!result) return NotFound();

            //return Ok();
            return RedirectToAction("Index", "Tasks");
        }
    }
}