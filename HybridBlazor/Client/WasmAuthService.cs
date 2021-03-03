using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HybridBlazor.Shared;

namespace HybridBlazor.Client
{
    public class WasmAuthService : IAuthService
    {
        private readonly HttpClient httpClient;

        public WasmAuthService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<CurrentUser> CurrentUserInfo()
        {
            return await httpClient.GetFromJsonAsync<CurrentUser>("api/Auth/currentUserInfo");
        }

        public async Task<string> Login(LoginRequest loginRequest, string _)
        {
            var result = await httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);

            if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception(await result.Content.ReadAsStringAsync());

            result.EnsureSuccessStatusCode();

            return string.Empty;
        }

        public async Task<string> Logout()
        {
            var result = await httpClient.PostAsync("api/Auth/Logout", null);

            result.EnsureSuccessStatusCode();

            return null;
        }
    }
}
