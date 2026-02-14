using CRM.DTOS;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Controllers
{
    public class AccountController : Controller
    {
        public UserManager<ApplicationUser> userManager;
        public SignInManager<ApplicationUser> _signInManager;
        public JwtService jwtService;

        public AccountController(UserManager<ApplicationUser> userManager, JwtService jwtService, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.jwtService = jwtService;
            _signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser()
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await userManager.AddToRoleAsync(user, "SalesExecutive");
            return RedirectToAction("login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);

            Console.WriteLine(user);

            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError("", "Invalid Credentials");
                return View(model);
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles);

            Response.Cookies.Append("cookie", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                //Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            TempData["Successmessage"] = "Logged In Successfully";

            string landingUrl = "/Home/Index";

            if (await userManager.IsInRoleAsync(user, "Admin"))
                landingUrl = "/Admin/Dashboard";
            else if (await userManager.IsInRoleAsync(user, "SalesManager"))
                landingUrl = "/SalesManager/Dashboard";



            return LocalRedirect(landingUrl);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            Response.Cookies.Delete("cookie");
            TempData["Successmessage"] = "Logged Out Successfully";
            return RedirectToAction("Index","Home");
        }
    }
}
