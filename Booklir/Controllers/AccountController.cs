using AutoMapper;
using Booklir.Core.Interfaces;
using Booklir.extensions;
using Booklir.Models;
using Booklir.Queries;
using Booklir.ViewModels.Authentication;
using Booklir.ViewModels.Booking;
using Booklir.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booklir.Controllers
{
    public class AccountController : Controller
    {

        private readonly IAuthService _authService;
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageStorageService _imageStorageService;
        private readonly IMapper _mapper;

        public AccountController(IAuthService authService, IMapper mapper, UserManager<ApplicationUser> userManager, IImageStorageService imageStorageService, IBookingService bookingService)
        {
            _authService = authService;
            _mapper = mapper;
            _userManager = userManager;
            _imageStorageService = imageStorageService;
            _bookingService = bookingService;
        }

        [HttpGet] public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Login(LoginViewModel model, string? returnUrl = null) 
        {
            if (!ModelState.IsValid)
            {
                // Return bad request if validation fails for AJAX requests
                if (Request.IsAjaxRequest())
                {
                    return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                }
          
                return View(model);
            }

              var result = await _authService.LoginAsync(model);
              if(result.Success) 
                {
                // If the login is successful, redirect to the return URL or home page 
                // So Return Json for AJAX requests
                if (Request.IsAjaxRequest())
                {
                    return Json(new { redirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/") });
                }
                // For non-AJAX requests, redirect to the return URL or home page
                   return LocalRedirect(returnUrl ?? Url.Content("~/"));
                }

            if (Request.IsAjaxRequest())
                return BadRequest(result.Errors.FirstOrDefault());

            // If the login fails, add errors to the model state
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        
        }

        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) 
        {
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                    // send back the first error message
                    return BadRequest(ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .First()
                                        .ErrorMessage);

                return View(model);
            }
            var result = await _authService.RegisterAsync(model);
            if (result.Success)
            {
                if(Request.IsAjaxRequest())
                    return Json(new { redirectUrl = Url.Action("Index","Home") });

                return RedirectToAction("Index", "Home");
            }


            if (Request.IsAjaxRequest())
                // return the first error so the client can toast it
                return BadRequest(result.Errors.First());

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout() 
        {
           await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Map ApplicationUser → EditProfileViewModel
            var vm = _mapper.Map<EditProfileViewModel>(user);

            vm.ThemeOptions = GetThemeOptions();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.ProfileImageUrl = user.ProfileImageUrl;
                model.ThemeOptions = GetThemeOptions();
                return View(model);
            }

            // Manual mapping for reliable updates
            user.ThemePreference = model.ThemePreference;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var imageUrl = await _imageStorageService.UploadImageAsync(model.ProfileImage, "profile");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        await _imageStorageService.DeleteImageAsync(user.ProfileImageUrl);
                    }
                    user.ProfileImageUrl = imageUrl;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["UpdatedTheme"] = model.ThemePreference;
                TempData["ThemePreference"] = user.ThemePreference;
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(EditProfile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.ProfileImageUrl = user.ProfileImageUrl;
            model.ThemeOptions = GetThemeOptions();
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Manage(int page = 1, int pageSize = 10)
        {
            // Fetch current user 
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Build the EditProfileViewModel
            var profileVm = _mapper.Map<EditProfileViewModel>(user);
            profileVm.ThemeOptions = GetThemeOptions();

            // Fetch user's bookings (Paged)
            var bookingParams = new BookingQueryParameters
            {
                UserEmail = user.Email,
                Page = page,
                PageSize = pageSize
            };
            var pagedBookings = await _bookingService.GetBookingAsync(bookingParams);

            // Map bookings to paged view models
            var bookingListVm = new BookingListViewModel
            {
                Items = _mapper.Map<IEnumerable<BookingViewModel>>(pagedBookings.Items),
                TotalCount = pagedBookings.TotalCount,
                Parameters = bookingParams
            };

            //Combine into a single “Manage” VM
            var vm = new ManageAccountViewModel
            {
                Profile = profileVm,
                Bookings = bookingListVm
            };

            return View(vm);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageAccountViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();


            ModelState.Remove("Profile.Id");
            ModelState.Remove("Profile.Email");
            ModelState.Remove("Bookings.Items");
            ModelState.Remove("Bookings.TotalCount");

            // Keep the existing image URL around in case no new upload:
            vm.Profile.ProfileImageUrl = user.ProfileImageUrl;

            // If any validation fails, fall back into the same GET-style View
            if (!ModelState.IsValid)
            {
                vm.Profile.ThemeOptions = GetThemeOptions();
                await RehydrateBookings(vm);
                return View(vm);
            }

            // Map over the three fields:
            user.ThemePreference = vm.Profile.ThemePreference;
            user.FirstName = vm.Profile.FirstName;
            user.LastName = vm.Profile.LastName;

            // Handle image upload/delete as before…
            if (vm.Profile.ProfileImage != null && vm.Profile.ProfileImage.Length > 0)
            {
                var imageUrl = await _imageStorageService
                                     .UploadImageAsync(vm.Profile.ProfileImage, "profile");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                        await _imageStorageService.DeleteImageAsync(user.ProfileImageUrl);

                    user.ProfileImageUrl = imageUrl;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                vm.Profile.ProfileImageUrl = user.ProfileImageUrl;
                vm.Profile.ThemeOptions = GetThemeOptions();
                await RehydrateBookings(vm);
                return View(vm);
            }

            // ← THIS IS THE KEY: redirect to your GET Manage
            TempData["UserUpdated"] = "Profile updated successfully!";
            return RedirectToAction(
                nameof(Manage),
                new { page = vm.Bookings.Parameters.Page, pageSize = vm.Bookings.Parameters.PageSize }
            );
        }

        // helper to re-fetch and repopulate the Bookings sub-VM:
        private async Task RehydrateBookings(ManageAccountViewModel vm)
        {
            var bookingParams = new BookingQueryParameters
            {
                UserEmail = (await _userManager.GetUserAsync(User))!.Email,
                Page = vm.Bookings.Parameters.Page,
                PageSize = vm.Bookings.Parameters.PageSize
            };

            var paged = await _bookingService.GetBookingAsync(bookingParams);
            vm.Bookings = new BookingListViewModel
            {
                Items = _mapper.Map<IEnumerable<BookingViewModel>>(paged.Items),
                TotalCount = paged.TotalCount,
                Parameters = bookingParams
            };
        }

        private List<SelectListItem> GetThemeOptions() => new()
{
    new SelectListItem("Light", "light"),
    new SelectListItem("Dark", "dark")
};
    }
}
