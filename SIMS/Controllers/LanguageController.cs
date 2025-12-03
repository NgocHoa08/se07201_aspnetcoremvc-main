using Microsoft.AspNetCore.Mvc;
using SIMS.Services;

namespace SIMS.Controllers
{
    public class LanguageController : Controller
    {
        private readonly LanguageService _languageService;

        public LanguageController(LanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpPost]
        public IActionResult ChangeLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture))
            {
                culture = "en";
            }

            _languageService.SetLanguage(culture);

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Request.Headers["Referer"].ToString() ?? "/";
            }

            return LocalRedirect(returnUrl);
        }
    }
}