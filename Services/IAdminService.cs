using CRM.DTOS.admin;
using CRM.Models;

namespace CRM.Services
{
    public interface IAdminService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string userId);
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
        Task<bool> DeleteUserAsync(string userId);
    }
}