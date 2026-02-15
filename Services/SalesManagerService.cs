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

        public async Task<DashboardBaseDto> GetDashboardDataAsync(String userId,bool isManager)
        {
            if (isManager)
            {
                return new ManagerDashboardDto
                {
                    TotalCustomers = await GetTotalCustomersAsync(),
                    RecentlyAddedCustomers = await GetRecentlyAddedCustomersAsync(null),
                    UnassignedCustomers = await GetUnassignedCustomersCountAsync(),
                    ActiveTeamMembers = await GetActiveMembersAsync(),
                    CustomerDistribution = await GetCustomerDistribution()
                };
            }
            else
            {
                // Build the Executive version
                return new ExecutiveDashboardDto
                {
                    RecentlyAddedCustomers = await GetRecentlyAddedCustomersAsync(userId),
                    RecentTasks = await GetRecentTasks(userId)
                };
            }
        }

        public async Task<IEnumerable<CustomerDashboardDto>> GetRecentlyAddedCustomersAsync(String userId)
        {
            try
            {
                var query = _context.Customers.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(c => c.SalesRepId == userId);
                }

                var customers = await query
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(6)
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

        public async Task<Dictionary<string, int>> GetCustomerDistribution()
        {
            try
            {
                var distribution = await _context.Customers
                    .Where(c => !string.IsNullOrEmpty(c.SalesRepId))
                    .Join(_context.Users,
                          c => c.SalesRepId,
                          u => u.Id,
                          (c, u) => new { u.FullName })
                    .GroupBy(x => x.FullName)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(x => x.Name, x => x.Count);

                return distribution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer distribution");
                return new Dictionary<string, int>();
            }
        }

        public Task<List<Activity>> GetRecentTasks(String userId)
        {
            try
            {
                var tasks = _context.Tasks
                    .Where(a => a.AssignTo == userId)
                    .OrderByDescending(a => a.DueDate)
                    .Take(6)
                    .ToListAsync();
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching recent tasks: {ex.Message}");
                return Task.FromResult(new List<Activity>());
            }
            }
    }

    }
