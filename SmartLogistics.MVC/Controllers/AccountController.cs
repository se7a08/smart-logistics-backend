using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Infrastructure.Identity;
using SmartLogistics.MVC.Models;

namespace SmartLogistics.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signIn;

        public AccountController(SignInManager<ApplicationUser> signIn) => _signIn = signIn;

        [HttpGet, AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signIn.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");

            ModelState.AddModelError(string.Empty, result.IsLockedOut
                ? "الحساب مقفول مؤقتًا، حاول بعدين."
                : "الإيميل أو الباسورد غلط.");

            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}

