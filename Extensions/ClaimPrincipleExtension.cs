using System.Security.Claims;

namespace CRM.Extensions
{
    public static class ClaimPrincipleExtension
    {
        public static string GetDashBoardPage(this ClaimsPrincipal user)
        {
            if (user == null) return "/Home/Index";

            var claim = user.FindFirst("DashBoardPage");

            return claim?.Value ?? "/Home/Index";
        }
    }
}
