using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.Models;
using CRM.DTOS.admin;
namespace CRM.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAdminService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDto = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Add(new UserListDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "No Role"
                });

                return userDto;
            }

            return userDto;
        }
        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(newRole)) throw new Exception("Role does not exist");

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removedRoles = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removedRoles.Succeeded) throw new Exception("Failed to remove existing roles");

            }

            var res = _userManager.AddToRoleAsync(user, newRole);
            return res.Result.Succeeded;

        }
    }
}
