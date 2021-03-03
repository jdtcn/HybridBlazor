using System.Threading.Tasks;

namespace HybridBlazor.Shared
{
    public interface IAuthService
    {
        Task<string> Login(LoginRequest loginRequest, string returnUrl);
        Task<string> Logout();
        Task<CurrentUser> CurrentUserInfo();
    }
}
