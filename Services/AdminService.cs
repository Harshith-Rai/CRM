using CRM.Data;
using CRM.DTOS.admin;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
namespace CRM.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly ISalesManagerService _salesManagerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminService(AppDbContext context,ISalesManagerService salesManagerService,UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _salesManagerService = salesManagerService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var now = DateTime.UtcNow;
            var startDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
            var sixMonthsAgo = DateTime.SpecifyKind(now.AddMonths(-5), DateTimeKind.Utc);

            // --- 1. PREPARE TREND DATA (Fixes "Empty" Chart) ---
            // Create a list of the last 6 months explicitly (e.g., Sep, Oct, Nov, Dec, Jan, Feb)
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => sixMonthsAgo.AddMonths(i))
                .Select(d => new { Year = d.Year, Month = d.Month, Label = d.ToString("MMM") })
                .ToList();

            // Fetch actual data from DB
            var dbGrowth = await _context.Customers
                .Where(c=>c.IsActive && c.CreatedAt >= sixMonthsAgo)
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            // Merge: If DB has no data for a month, use 0
            var monthlyGrowth = new Dictionary<string, int>();
            foreach (var m in last6Months)
            {
                var match = dbGrowth.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
                monthlyGrowth[m.Label] = match?.Count ?? 0;
            }
            // ----------------------------------------------------

            return new DashboardViewModel
            {
                TotalCustomers = await _context.Customers.CountAsync(),

                TotalContacts = await _context.Contacts
                    .Include(c => c.Customer)
                    .CountAsync(),

                NewCustomersThisMonth = await _context.Customers
                    .CountAsync(c=>c.CreatedAt >= startDate),

                //TotalPipelineValue = await _context.Leads
                //    .Where(l => l.SalesRepId == userId && l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost)
                //    .SumAsync(l => (decimal?)l.Value) ?? 0,

                //TotalRevenueWon = await _context.Leads
                //    .Where(l => l.SalesRepId == userId && l.Status == LeadStatus.Won)
                //    .SumAsync(l => (decimal?)l.Value) ?? 0,

                //RecentActivities = await _context.Notes
                //    .Include(n => n.Customer)
                //    .Where(n => n.AuthorId == userId && n.Customer.IsActive)
                //    .OrderByDescending(n => n.CreatedAt)
                //    .Take(5)
                //    .ToListAsync(),

                CustomersByIndustry = await _context.Customers
                    .GroupBy(c => c.Industry)
                    .Select(g => new { Industry = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Industry, x => x.Count),

                MonthlyGrowth = monthlyGrowth,

                RecentlyAddedCustomers = await _salesManagerService.GetRecentlyAddedCustomersAsync(null)
            };
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
                    await _context.Customers.Where(c => c.SalesRepId == userId).ExecuteUpdateAsync(s => s.SetProperty(c => c.SalesRepId, (string)null));
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
            catch (Exception ex)
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

                await _context.Customers.Where(c => c.SalesRepId == userId).ExecuteUpdateAsync(s => s.SetProperty(c => c.SalesRepId, (string)null));

                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<IdentityResult> RegisterNewUser(AddUserViewModel model)
        {
            var user = new ApplicationUser
            {
                Email = model.Email,
                FullName=model.FullName,
                UserName=model.Email,
                EmailConfirmed=true
            };

            var res =await  _userManager.CreateAsync(user, model.Password);
            if (res.Succeeded)
            {
                if (!String.IsNullOrEmpty(model.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }
            }

            return res;
        }
    }
}