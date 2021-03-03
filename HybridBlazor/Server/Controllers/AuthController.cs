using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using HybridBlazor.Shared;
using HybridBlazor.Server.Data.Models;

namespace HybridBlazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await userManager.FindByNameAsync(request.UserName);
            if (user == null) return BadRequest("Sorry, wrong username or password");
            var singInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!singInResult.Succeeded) return BadRequest("Sorry, wrong username or password");
            await signInManager.SignInAsync(user, request.RememberMe);
            return Ok();
        }

        [HttpGet("/[action]")]
        public async Task<IActionResult> SignInActual(string t)
        {
            var data = t;
            var parts = data.Split('|');

            var identityUser = await userManager.FindByIdAsync(parts[0]);

            var isTokenValid = await userManager.VerifyUserTokenAsync(identityUser, TokenOptions.DefaultProvider, "SignIn", parts[1]);

            if (isTokenValid)
            {
                await signInManager.SignInAsync(identityUser, true);
                if (parts.Length == 3 && Url.IsLocalUrl(parts[2]))
                {
                    return Redirect(parts[2]);
                }
                return Redirect("/");
            }
            else
            {
                return Unauthorized("STOP!");
            }
        }

        [HttpGet]
        public CurrentUser CurrentUserInfo()
        {
            return new CurrentUser
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                Claims = User.Claims.ToDictionary(c => c.Type, c => c.Value)
            };
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("/logout")]
        public async Task<IActionResult> LogoutServer()
        {
            await signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}
