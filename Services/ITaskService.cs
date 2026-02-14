using CRM.DTOS.admin;
using CRM.Models;

namespace CRM.Services
{
    public interface ITaskService
    {
        Task CreateTask(Activity task);
        Task<bool> UpdateStatus(int id, bool isCompleted);
        Task<List<ApplicationUser>> GetAssignableUsers(string userId,string role);

        Task<List<Activity>> GetAllTasks(String userId);
    }
}
