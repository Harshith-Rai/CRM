using CRM.DTOS;
using CRM.Models;
using CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace CRM.Controllers
{
    public class AccountController : Controller
    {
        public UserManager<ApplicationUser> userManager;
        public SignInManager<ApplicationUser> _signInManager;
        public JwtService jwtService;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            JwtService jwtService,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.jwtService = jwtService;
            _signInManager = signInManager;
            _emailSender = emailSender;
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
                Secure = true
            });

            TempData["Successmessage"] = "Logged In Successfully";

            string landingUrl = "/Home/Index";

            if (await userManager.IsInRoleAsync(user, "Admin"))
                landingUrl = "/Admin/Dashboard";
            else if (await userManager.IsInRoleAsync(user, "SalesManager"))
                landingUrl = "/SalesManager/Dashboard";



            return LocalRedirect(landingUrl);
        }
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required");
                return View();
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Security: don't reveal user doesn't exist, just redirect to the verification page
                return RedirectToAction("VerifyOtp", new { email = email });
            }

            // Generate 6-digit OTP
            var otpCode = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

            var html = $@"
                <div style='font-family: sans-serif; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #333;'>Password Reset Code</h2>
                    <p>Enter the code below on the verification page to reset your password:</p>
                    <div style='font-size: 32px; font-weight: bold; color: #e74c3c; letter-spacing: 5px; padding: 10px;'>{otpCode}</div>
                    <p style='color: #777; font-size: 12px;'>This code is valid for a limited time.</p>
                </div>";

            await _emailSender.SendEmailAsync(email, "Your Reset Code", html);

            return RedirectToAction("VerifyOtp", new { email = email });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("Login");

            // Verify the OTP
            var isValid = await userManager.VerifyTwoFactorTokenAsync(user, "Email", model.Code);

            if (isValid)
            {
                // Generate the actual Reset Token for the next step
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                // Encode for URL safety
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                return RedirectToAction("ResetPassword", new { token = encodedToken, email = model.Email });
            }

            ModelState.AddModelError("", "Invalid or expired verification code.");
            return View(model);
        }

        // --- RESET PASSWORD ---

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var model = new ResetPasswordDto { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Message"] = "Password reset successful.";
                return RedirectToAction("Login");
            }

            // Decode the token back to its original form
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await userManager.ResetPasswordAsync(user, decodedToken, model.Password);

            if (result.Succeeded)
            {
                TempData["Message"] = "Password reset successful. You can sign in now.";
                return RedirectToAction("Login");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return View(model);
        }
        [HttpPost]
        public ActionResult Logout()
        {
            Response.Cookies.Delete("cookie");
            TempData["Successmessage"] = "Logged Out Successfully";
            return RedirectToAction("login");
        }
    }
}
