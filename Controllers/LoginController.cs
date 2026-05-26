using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RentIO.ViewModels;
using System.Security.Claims;

namespace RentIO.Controllers
{
    public class LoginController : Controller
    {
        private const string AdminKorisnickoIme = "admin";
        private const string AdminLozinka = "admin123";

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (vm.KorisnickoIme != AdminKorisnickoIme || vm.Lozinka != AdminLozinka)
            {
                ModelState.AddModelError(string.Empty, "Pogrešno korisničko ime ili lozinka.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, AdminKorisnickoIme),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
