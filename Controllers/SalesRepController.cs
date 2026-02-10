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

                DealsInProposal = 4,
                DealsInNegotiation = 2,
                DealsClosedWon = 7
            };

            return View(model);
        }
    }
}