using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRM.Models;
using System.Collections.Generic;

namespace CRM.Controllers
{
    // [Authorize(Roles = "SalesRep")] // Uncomment this later when Roles are set up
    public class SalesRepController : Controller
    {
        public IActionResult Index()
        {
            // MOCK DATA - Simulating database results
            var model = new SalesRepDashboardViewModel
            {
                TotalSalesThisMonth = 12500,
                MonthlyTarget = 20000,

                TodaysTasks = new List<TaskItem>
                {
                    new TaskItem { Title = "Call John Doe", Subtitle = "Discuss contract renewal", Type = "Call" },
                    new TaskItem { Title = "Email Sarah Smith", Subtitle = "Send Q3 Proposal", Type = "Email" }
                },

                HotLeads = new List<LeadItem>
                {
                    new LeadItem { Name = "Tech Solutions Ltd", Company = "IT Sector", Score = 95 },
                    new LeadItem { Name = "Green Energy Inc", Company = "Power", Score = 88 }
                },
                RecentActivities = new List<ActivityItem>
                {
                    new ActivityItem { CustomerName = "Tech Solutions Ltd", Action = "Logged a call regarding Q3", TimeAgo = "2h ago", Type = "Call" },
                    new ActivityItem { CustomerName = "Green Energy Inc", Action = "Updated status to Negotiation", TimeAgo = "5h ago", Type = "Status" },
                    new ActivityItem { CustomerName = "Alpha Corp", Action = "Added a new contact: Mike Ross", TimeAgo = "1d ago", Type = "Contact" }
                },

                DealsInProposal = 4,
                DealsInNegotiation = 2,
                DealsClosedWon = 7
            };


            return View(model);
        }
    }
}
