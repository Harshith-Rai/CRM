using CRM.DTOS.Admin;

namespace CRM.Services
{
    public interface IUserAdminService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
    }
}
