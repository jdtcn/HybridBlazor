using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using HybridBlazor.Shared;

namespace HybridBlazor.Client
{
    public class HostAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService api;
        private CurrentUser _currentUser;

        public HostAuthenticationStateProvider(IAuthService api)
        {
            this.api = api;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await GetCurrentUser();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, _currentUser.UserName) }
                        .Concat(_currentUser.Claims.Select(c => new Claim(c.Key, c.Value)));

                    identity = new ClaimsIdentity(claims, nameof(HostAuthenticationStateProvider));
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex.ToString());
            }
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private async Task<CurrentUser> GetCurrentUser()
        {
            if (_currentUser != null && _currentUser.IsAuthenticated) return _currentUser;
            _currentUser = await api.CurrentUserInfo();
            return _currentUser;
        }

        public async Task<string> Logout()
        {
            var result = await api.Logout();
            _currentUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return result;
        }

        public async Task<string> Login(LoginRequest loginParameters, string returnUrl)
        {
            var result = await api.Login(loginParameters, returnUrl);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return result;
        }
    }
}
