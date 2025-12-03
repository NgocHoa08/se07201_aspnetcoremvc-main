using Microsoft.AspNetCore.Localization;

namespace SIMS.Services
{
    public class LanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetLanguage(string culture)
        {
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture));

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                cookieValue,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/"
                }
            );
        }

        public string GetCurrentLanguage()
        {
            var feature = _httpContextAccessor.HttpContext?
                .Features.Get<IRequestCultureFeature>();
            return feature?.RequestCulture.Culture.Name ?? "en";
        }
    }
}
