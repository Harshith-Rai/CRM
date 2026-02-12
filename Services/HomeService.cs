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

            // --- 1. PREPARE TREND DATA (Fixes "Empty" Chart) ---
            // Create a list of the last 6 months explicitly (e.g., Sep, Oct, Nov, Dec, Jan, Feb)
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => sixMonthsAgo.AddMonths(i))
                .Select(d => new { Year = d.Year, Month = d.Month, Label = d.ToString("MMM") })
                .ToList();

            // Fetch actual data from DB
            var dbGrowth = await _context.Customers
                .Where(c => c.SalesRepId == userId && c.IsActive && c.CreatedAt >= sixMonthsAgo)
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
                TotalCustomers = await _context.Customers.CountAsync(c => c.SalesRepId == userId && c.IsActive),

                TotalContacts = await _context.Contacts
                    .Include(c => c.Customer)
                    .CountAsync(c => c.Customer.SalesRepId == userId && c.Customer.IsActive),

                NewCustomersThisMonth = await _context.Customers
                    .CountAsync(c => c.SalesRepId == userId && c.IsActive && c.CreatedAt >= startDate),

                //TotalPipelineValue = await _context.Leads
                //    .Where(l => l.SalesRepId == userId && l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost)
                //    .SumAsync(l => (decimal?)l.Value) ?? 0,

                //TotalRevenueWon = await _context.Leads
                //    .Where(l => l.SalesRepId == userId && l.Status == LeadStatus.Won)
                //    .SumAsync(l => (decimal?)l.Value) ?? 0,

                RecentActivities = await _context.Notes
                    .Include(n => n.Customer)
                    .Where(n => n.AuthorId == userId && n.Customer.IsActive)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                CustomersByIndustry = await _context.Customers
                    .Where(c => c.SalesRepId == userId && c.IsActive && c.Industry != null)
                    .GroupBy(c => c.Industry)
                    .Select(g => new { Industry = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Industry, x => x.Count),

                MonthlyGrowth = monthlyGrowth
            };
        }
    }
}