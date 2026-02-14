using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRM.Services
{
    public class TaskService:ITaskService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public TaskService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task CreateTask(Activity task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatus(int id, bool isCompleted)
        {
            var activity = await _context.Tasks.FindAsync(id);
            if (activity == null) return false;

            activity.Status = isCompleted;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ApplicationUser>> GetAssignableUsers(string userId, string role)
        {
            try
            {
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null) return new List<ApplicationUser>();

                var assignableList = new List<ApplicationUser>();

                // 1. Admin Logic: Sees everyone
                if (role == "Admin")
                {
                    return await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
                }

                if (role == "SalesManager")
                {
                    assignableList.Add(currentUser);
                    var executives = await _userManager.GetUsersInRoleAsync("SalesExecutive");
                    assignableList.AddRange(executives.Where(e => e.Id != userId));
                    return assignableList.OrderBy(u => u.FullName).ToList();
                }

                assignableList.Add(currentUser);
                return assignableList;
            }
            catch (Exception)
            {
                return new List<ApplicationUser>();
            }
        }

        public async Task<List<Activity>> GetAllTasks(String userId)
        {
            try
            {
                return await _context.Tasks.Where(t => t.AssignTo == userId).ToListAsync();
            }
            catch(Exception e)
            {
                return new List<Activity> { };
            }
        }
    }


}
