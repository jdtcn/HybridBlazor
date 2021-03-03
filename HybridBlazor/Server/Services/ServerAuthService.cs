using HybridBlazor.Server.Data.Models;
using HybridBlazor.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HybridBlazor.Client
{
    public class ServerAuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ServerAuthService(IHttpContextAccessor httpContextAccessor, 
            UserManager<ApplicationUser> userManager)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }

        public Task<CurrentUser> CurrentUserInfo()
        {
            var user = httpContextAccessor.HttpContext.User;
            return Task.FromResult(new CurrentUser
            {
                IsAuthenticated = user.Identity.IsAuthenticated,
                UserName = user.Identity.Name,
                Claims = user.Claims
                .ToDictionary(c => c.Type, c => c.Value)
            });
        }

        public async Task<string> Login(LoginRequest loginRequest, string returnUrl)
        {
            var user = await userManager.FindByEmailAsync(loginRequest.UserName);

            if (user != null && await userManager.CheckPasswordAsync(user, loginRequest.Password))
            {

                var token = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "SignIn");

                var data = $"{user.Id}|{token}";

                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    data += $"|{returnUrl}";
                }

                return data;
            }
            else
            {
                throw new Exception("Sorry, wrong username or password");
            }
        }

        public Task<string> Logout()
        {
            return Task.FromResult("logoutServer");
        }
    }
}
