using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Services;
using System.Security.Claims;

namespace SIMS.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserService _userService;

        public SettingsController(UserService userService)
        {
            _userService = userService;
        }

        // GET: Settings
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userService.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(user);
        }

        // GET: Settings/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userService.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(user);
        }

        // POST: Settings/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string phoneNumber)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userService.GetUserByUsernameAsync(username);

            if (user != null)
            {
                user.FullName = fullName;
                user.Email = email;
                user.PhoneNumber = phoneNumber;
                user.UpdatedDate = DateTime.Now;

                var result = await _userService.UpdateUserAsync(user);
                if (result)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error updating profile!";
                }
            }

            return RedirectToAction("Profile");
        }

        // GET: Settings/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match!";
                return View();
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userService.GetUserByUsernameAsync(username);

            if (user != null)
            {
                if (user.Password != currentPassword)
                {
                    TempData["ErrorMessage"] = "Current password is incorrect!";
                    return View();
                }

                user.Password = newPassword;
                user.UpdatedDate = DateTime.Now;

                var result = await _userService.UpdateUserAsync(user);
                if (result)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error changing password!";
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Settings/ActivityLog
        [HttpGet]
        public IActionResult ActivityLog()
        {
            // TODO: Implement activity log functionality
            return View();
        }
    }
}