using CRM.DTOS;
using CRM.DTOS.admin;
using CRM.Models;
using Microsoft.AspNetCore.Identity;

namespace CRM.Services
{
    public interface IAdminService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string userId);
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
        Task<bool> DeleteUserAsync(string userId);
        Task<IdentityResult> RegisterNewUser(AddUserViewModel model);
    }
}