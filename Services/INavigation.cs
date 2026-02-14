using System.Security.Claims;

namespace CRM.Services
{
    public interface INavigation
    {
        string GetDashboardUrl(ClaimsPrincipal user);
    }
}
