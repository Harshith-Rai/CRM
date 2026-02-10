using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Services
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _context;

        public HomeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var now = DateTime.UtcNow;
            var startDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
            var sixMonthsAgo = DateTime.SpecifyKind(now.AddMonths(-5), DateTimeKind.Utc);

            return new DashboardViewModel
            {
                TotalCustomers = await _context.Customers.CountAsync(c => c.SalesRepId == userId && c.IsActive),
                TotalContacts = await _context.Contacts.Include(c => c.Customer).CountAsync(c => c.Customer.SalesRepId == userId),

                NewCustomersThisMonth = await _context.Customers
                    .CountAsync(c => c.SalesRepId == userId && c.IsActive && c.CreatedAt >= startDate),

                TotalPipelineValue = await _context.Leads
                    .Where(l => l.SalesRepId == userId && l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost)
                    .SumAsync(l => l.Value),

                TotalRevenueWon = await _context.Leads
                    .Where(l => l.SalesRepId == userId && l.Status == LeadStatus.Won)
                    .SumAsync(l => l.Value),

                RecentActivities = await _context.Notes
                    .Include(n => n.Customer)
                    .Where(n => n.AuthorId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                CustomersByIndustry = await _context.Customers
                    .Where(c => c.SalesRepId == userId && c.IsActive && c.Industry != null)
                    .GroupBy(c => c.Industry)
                    .Select(g => new { Industry = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Industry, x => x.Count),

                MonthlyGrowth = await _context.Customers
                    .Where(c => c.SalesRepId == userId && c.IsActive && c.CreatedAt >= sixMonthsAgo)
                    .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                    .Select(g => new {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToDictionaryAsync(x => x.Date.ToString("MMM"), x => x.Count)
            };
        }
    }
}