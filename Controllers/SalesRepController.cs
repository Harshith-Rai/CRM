using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.Data;   // Needed for AppDbContext
using CRM.Models; // Needed for Models and LeadStatus Enum
using System.Security.Claims;

namespace CRM.Controllers
{
    [Authorize] // 🔒 Protects this page
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

            // ---------------------------------------------------------
            // 1. FETCH REAL DATA FROM DB
            // ---------------------------------------------------------

            // A. Customers Query
            var myCustomersQuery = _context.Customers
                .Where(c => c.SalesRepId == userId);

            // B. Notes Query (for Tasks & Activities)
            var myNotesQuery = _context.Notes
                .Include(n => n.Customer)
                .Where(n => n.AuthorId == userId);

            // C. Leads Query (for Pipeline)
            var myLeadsQuery = _context.Leads
                .Where(l => l.SalesRepId == userId);

            // ---------------------------------------------------------
            // 2. CALCULATE METRICS
            // ---------------------------------------------------------

            // --- Customer Stats ---
            var totalCustomers = await myCustomersQuery.CountAsync(c => c.IsActive);

            var newThisMonth = await myCustomersQuery
                .CountAsync(c => c.CreatedAt.Month == DateTime.UtcNow.Month &&
                                 c.CreatedAt.Year == DateTime.UtcNow.Year);

            var totalContacts = await myCustomersQuery
                .SelectMany(c => c.Contacts)
                .CountAsync();

            // --- Pipeline Stats (FIXED: Using Enums) ---
            // 1. Count deals by Status Enum
            var dealsProposal = await myLeadsQuery.CountAsync(l => l.Status == LeadStatus.Proposal);
            var dealsNegotiation = await myLeadsQuery.CountAsync(l => l.Status == LeadStatus.Negotiation);
            var dealsWon = await myLeadsQuery.CountAsync(l => l.Status == LeadStatus.Won);

            // 2. Financials
            // Revenue = Sum of "Won" deals
            var revenueWon = await myLeadsQuery
                .Where(l => l.Status == LeadStatus.Won)
                .SumAsync(l => l.Value);

            // Pipeline Value = Sum of active deals (Proposal + Negotiation + Qualification)
            var pipelineValue = await myLeadsQuery
                .Where(l => l.Status == LeadStatus.Proposal ||
                            l.Status == LeadStatus.Negotiation ||
                            l.Status == LeadStatus.Qualification)
                .SumAsync(l => l.Value);


            // ---------------------------------------------------------
            // 3. FETCH CHARTS DATA
            // ---------------------------------------------------------

            // Industry Breakdown
            var industryData = await myCustomersQuery
                .GroupBy(c => c.Industry)
                .Select(g => new { Industry = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Industry ?? "Unspecified", v => v.Count);

            // Monthly Growth (Last 6 Months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
            var growthData = await myCustomersQuery
                .Where(c => c.CreatedAt >= sixMonthsAgo)
                .GroupBy(c => new { c.CreatedAt.Month, c.CreatedAt.Year })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    Count = g.Count()
                })
                .ToListAsync();

            var monthlyGrowth = new Dictionary<string, int>();
            foreach (var item in growthData) monthlyGrowth[item.Month] = item.Count;


            // ---------------------------------------------------------
            // 4. FETCH LISTS (Tasks, Leads, Activity)
            // ---------------------------------------------------------

            // "Priority Tasks" = Notes with ReminderDate >= Today
            var tasks = await myNotesQuery
                .Where(n => n.ReminderDate != null && n.ReminderDate >= DateTime.UtcNow)
                .OrderBy(n => n.ReminderDate)
                .Take(5)
                .Select(n => new TaskItem
                {
                    Title = n.Title ?? "Follow Up",
                    Subtitle = $"Customer: {n.Customer.CompanyName}",
                    Type = "Call"
                })
                .ToListAsync();

            // "Hot Leads" = Active Leads (Not Won/Lost), ordered by newest
            // FIXED: Using Enums for comparison
            var hotLeads = await myLeadsQuery
                .Where(l => l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost)
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .Select(l => new LeadItem
                {
                    Name = l.Title ?? "New Opportunity",
                    Company = l.Source ?? "Unknown",
                    Score = 75 // Placeholder score
                })
                .ToListAsync();

            // "Recent Activity" = The last 5 notes
            var activities = await myNotesQuery
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new ActivityItem
                {
                    CustomerName = n.Customer.CompanyName,
                    Action = n.Content.Length > 50 ? n.Content.Substring(0, 50) + "..." : n.Content,
                    TimeAgo = GetTimeAgo(n.CreatedAt),
                    Type = "Note"
                })
                .ToListAsync();


            // ---------------------------------------------------------
            // 5. BUILD VIEW MODEL
            // ---------------------------------------------------------
            var model = new SalesRepDashboardViewModel
            {
                // KPI Stats
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newThisMonth,
                TotalContacts = totalContacts,

                // Pipeline Financials (Now Real Data)
                TotalSalesThisMonth = revenueWon, // Maps to "Revenue Won" card
                TotalPipelineValue = pipelineValue, // Maps to "Pipeline Value" card

                // Pipeline Counts (Now Real Data)
                DealsInProposal = dealsProposal,
                DealsInNegotiation = dealsNegotiation,
                DealsClosedWon = dealsWon,

                // Charts & Lists
                CustomersByIndustry = industryData,
                MonthlyGrowth = monthlyGrowth,
                TodaysTasks = tasks,
                HotLeads = hotLeads,
                RecentActivities = activities
            };

            return View(model);
        }

        // Helper to format "2h ago", "5m ago"
        private static string GetTimeAgo(DateTime date)
        {
            var span = DateTime.UtcNow - date;
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            return $"{(int)span.TotalDays}d ago";
        }
    }
}