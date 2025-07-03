using Booklir.Core.Interfaces;
using Booklir.Core.Results;
using Booklir.Models;
using Booklir.ViewModels.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Booklir.Core.Service
{
    public class AuthSerivce : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AuthSerivce(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
            
            return new AuthResult
            {
                Success = result.Succeeded,
                Errors = result.IsLockedOut ?
                 new[] { "Account locked" } :
                 result.IsNotAllowed ?
                 new[] { "Verify email first" } :
                 new[] { "Invalid credentials" }
            };
        }


        public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = true,
                ProfileImageUrl = "/images/admin-avatar-defualt.png",
                CreatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return new AuthResult { Success = true };
            }

            return new AuthResult { Success = false, Errors = result.Errors.Select(e => e.Description) };
        }
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
