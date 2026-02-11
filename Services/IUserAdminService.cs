
using CRM.DTOS.admin;

namespace CRM.Services
{
    public interface IUserAdminService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
        Task<bool> DeleteUserAsync(string userId);

    }
}
