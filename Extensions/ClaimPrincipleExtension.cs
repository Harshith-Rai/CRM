using System.Security.Claims;

namespace CRM.Extensions
{
    public static class ClaimPrincipleExtension
    {
        public static string GetDashBoardPage(this ClaimsPrincipal user)
        {
            if (user == null) return "/Home/Index";

            var claim = user.FindFirst("DashBoardPage");

            //throw new Exception($"Claim 'DashBoardPage' not found for user {user.Identity.Name} {claim.Value}");
            return claim?.Value ?? "/Home/Index";
        }
    }
}
