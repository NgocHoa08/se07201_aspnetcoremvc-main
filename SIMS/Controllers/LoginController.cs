using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Services;
using System.Security.Claims;

namespace SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserService _userService;

        public LoginController(UserService service)
        {
            _userService = service;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = model.Username;
                string password = model.Password;
                var userInfo = await _userService.LoginUser(username, password);

                if (userInfo != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userInfo.Username),
                        new Claim(ClaimTypes.Role, userInfo.Role),
                        new Claim(ClaimTypes.Email, userInfo.Email ?? ""),
                        new Claim("FullName", userInfo.FullName ?? "")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));

                    return RedirectToAction("Index", "Dashboard");
                }

                ViewData["InvalidAccount"] = "Invalid username or password!";
            }
            return View(model);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _userService.RegisterUser(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                    return RedirectToAction("Index");
                }
                else
                {
                    if (message.Contains("Username"))
                    {
                        ModelState.AddModelError("Username", message);
                    }
                    else if (message.Contains("Email"))
                    {
                        ModelState.AddModelError("Email", message);
                    }
                    else
                    {
                        ModelState.AddModelError("", message);
                    }
                }
            }
            return View(model);
        }

        // POST: Logout
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            return RedirectToAction("Index", "Login");
        }
    }
}