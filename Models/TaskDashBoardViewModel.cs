using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRM.Models
{
    public class TaskDashBoardViewModel
    {
        public List<Activity> Tasks { get; set; } = new();
        public SelectList AssigneeList { get; set; }
        public string CurrentFilter { get; set; }
    }
}
