using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models;

namespace CRM.Controllers
{
    [Authorize]
    public class SalesRepController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesRepController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var myCustomersQuery = _context.Customers
                .Where(c => c.SalesRepId == userId);

            var myNotesQuery = _context.Notes
                .Include(n => n.Customer)
                .Where(n => n.AuthorId == userId)
                .Where(n => n.Customer.IsActive); 
           
            var totalCustomers = await myCustomersQuery.CountAsync(c => c.IsActive);
            var newThisMonth = await myCustomersQuery.CountAsync(c => c.CreatedAt.Month == DateTime.UtcNow.Month && c.CreatedAt.Year == DateTime.UtcNow.Year);
            var totalContacts = await myCustomersQuery.SelectMany(c => c.Contacts).CountAsync();

            //var dealsProposal = await myLeadsQuery.CountAsync(l => l.Status == LeadStatus.Proposal);
            //var dealsNegotiation = await myLeadsQuery.CountAsync(l => l.Status == LeadStatus.Negotiation);
            //var dealsWon = await myLeadsQuery.CountAsync(l => l.Status == LeadStatus.Won);
            //var revenueWon = await myLeadsQuery.Where(l => l.Status == LeadStatus.Won).SumAsync(l => l.Value);
            //var pipelineValue = await myLeadsQuery.Where(l => l.Status == LeadStatus.Proposal || l.Status == LeadStatus.Negotiation || l.Status == LeadStatus.Qualification).SumAsync(l => l.Value);

            var industryData = await myCustomersQuery
                .Where(c => c.IsActive) 
                .GroupBy(c => c.Industry)
                .Select(g => new { Industry = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Industry ?? "Unspecified", v => v.Count);

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
            var growthData = await myCustomersQuery
                .Where(c => c.IsActive && c.CreatedAt >= sixMonthsAgo)
                .GroupBy(c => new { c.CreatedAt.Month, c.CreatedAt.Year })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"), Count = g.Count() })
                .ToListAsync();
            var monthlyGrowth = new Dictionary<string, int>();
            foreach (var item in growthData) monthlyGrowth[item.Month] = item.Count;

            var tasks = await myNotesQuery
                .Where(n => n.ReminderDate != null && n.ReminderDate >= DateTime.UtcNow.Date) 
                .Where(n => !n.IsReminderDone) 
                .OrderBy(n => n.ReminderDate)
                .Take(5)
                .Select(n => new TaskItem
                {
                    Id = n.Id, 
                    Title = n.Title ?? "Follow Up",
                    Subtitle = n.Customer.CompanyName,
                    Type = (n.Title.ToLower().Contains("call") ? "Call" :
                           (n.Title.ToLower().Contains("email") ? "Email" : "Meeting"))
                })
                .ToListAsync();

            // Hot Leads
            //var hotLeads = await myLeadsQuery
            //    .Where(l => l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost)
            //    .OrderByDescending(l => l.CreatedAt)
            //    .Take(5)
            //    .Select(l => new LeadItem { Name = l.Title, Company = l.Source, Score = 75 })
            //    .ToListAsync();

            // Recent Activity
            var activities = await myNotesQuery
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new ActivityItem
                {
                    CustomerName = n.Customer.CompanyName,
                    Action = n.Content.Length > 50 ? n.Content.Substring(0, 50) + "..." : n.Content,
                    TimeAgo = GetTimeAgo(n.CreatedAt),
                    Type = n.Title.Contains("Call") ? "Call" : "Note"
                })
                .ToListAsync();

            // 5. BUILD MODEL
            var model = new SalesRepDashboardViewModel
            {
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newThisMonth,
                TotalContacts = totalContacts,
                //TotalSalesThisMonth = revenueWon,
                //TotalPipelineValue = pipelineValue,
                //DealsInProposal = dealsProposal,
                //DealsInNegotiation = dealsNegotiation,
                //DealsClosedWon = dealsWon,
                CustomersByIndustry = industryData,
                MonthlyGrowth = monthlyGrowth,
                TodaysTasks = tasks,
                //HotLeads = hotLeads,
                RecentActivities = activities
            };
            return View(model);
        }

        private static string GetTimeAgo(DateTime date)
        {
            var span = DateTime.UtcNow - date;
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            return $"{(int)span.TotalDays}d ago";
        }
    }
}