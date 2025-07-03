using Booklir.Core.Results;
using Booklir.ViewModels.Authentication;

namespace Booklir.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterViewModel model);
        Task<AuthResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();

    }
}
