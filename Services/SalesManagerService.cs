using CRM.Data;
using CRM.DTOS.SalesManager;
using CRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CRM.Services
{
    public class SalesManagerService : ISalesManagerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SalesManagerService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public SalesManagerService(AppDbContext context, ILogger<SalesManagerService> logger,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<SalesManagerDashBoardDto> GetDashboardDataAsync()
        {
            var dashboardData = new SalesManagerDashBoardDto
            {
                TotalCustomers = await GetTotalCustomersAsync(),
                UnassignedCustomers = await GetUnassignedCustomersCountAsync(),
                ActiveTeamMembers = await GetActiveMembersAsync(),
                RecentlyAddedCustomers = await GetRecentlyAddedCustomersAsync()
            };
            return dashboardData;
        }

        public async Task<IEnumerable<CustomerDashboardDto>> GetRecentlyAddedCustomersAsync(int count = 6)
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(count)
                    .Select(c => new CustomerDashboardDto
                    {
                        Id = c.Id,
                        CompanyName = c.CompanyName,
                        CreatedAt = c.CreatedAt,
                        SalesRepId = c.SalesRepId,
                        Email = c.Email,
                    })
                    .ToListAsync();

                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching recently added customers: {ex.Message}");
                return Enumerable.Empty<CustomerDashboardDto>();
            }
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            try
            {
                return await _context.Customers.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error counting customers: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetUnassignedCustomersCountAsync()
        {
            try
            {
                return await _context.Customers.Where(c => String.IsNullOrEmpty(c.SalesRepId)).CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error counting unassigned customers: {ex.Message}");
                return 0;
            }



        }

        public async Task<int> GetActiveMembersAsync()
        {
            try
            {
                var salesRep=await _userManager.GetUsersInRoleAsync("SalesExecutive");
                _logger.LogInformation($"Found {salesRep.Count} active Sales Executives");
                Console.WriteLine("the number of sales rep is here"+salesRep);
                return salesRep.Count();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error counting active team members: {ex.Message}");
                return 0;
            }
        }
    }

    }
