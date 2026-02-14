using System.Security.Claims;

namespace CRM.Services
{
    public class NavigationService:INavigation
    {
        public string GetDashboardUrl(ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin")) return "/admin/dashboard";
            else  return "/salesmanager/dashboard";

        }
    }
}
