using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.Models;
using CRM.DTOS.admin;
using CRM.Data;
namespace CRM.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext context;
        public UserAdminService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this.context = context;
        }
        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            var userDto = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Skip admin users - only show non-admin users
                if (roles.Contains("Admin"))
                {
                    continue;
                }

                userDto.Add(new UserListDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "No Role"
                });
            }

            return userDto;
        }
        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                if (!await _roleManager.RoleExistsAsync(newRole)) throw new Exception("Role does not exist");

                if (newRole == "SalesManager")
                {
                    await context.Customers.Where(c => c.SalesRepId == userId).ExecuteUpdateAsync(s => s.SetProperty(c => c.SalesRepId, (string)null));
                }

                var currentRoles = await _userManager.GetRolesAsync(user);

                if (currentRoles.Any())
                {
                    var removedRoles = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removedRoles.Succeeded) throw new Exception("Failed to remove existing roles");

                }

                var res = _userManager.AddToRoleAsync(user, newRole);
                return res.Result.Succeeded;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                await context.Customers.Where(c => c.SalesRepId == userId).ExecuteUpdateAsync(s => s.SetProperty(c => c.SalesRepId, (string)null));

                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
