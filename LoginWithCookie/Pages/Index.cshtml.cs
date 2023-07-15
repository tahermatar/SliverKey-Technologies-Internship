using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LoginWithCookie.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public LoginInput? LoginInput { get; set; } 
        private readonly string _loginPath = "/Index";
        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPost()
        {
            string userName = LoginInput.UserName;
            string password = LoginInput.Password;
            
            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("LoginError", "Username and password are required.");
                return Page();
            }

            if (userName == "intern" && password == "summer 2023 july")
            {

                List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, "User")
            };

                ClaimsIdentity identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToPage();

            } else
            {
                ModelState.AddModelError("LoginError", "Invalid username or password.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToPage(_loginPath);
        }
    }

    public class LoginInput
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}