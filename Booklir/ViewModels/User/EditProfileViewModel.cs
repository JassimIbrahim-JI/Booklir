using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.User
{
    public class EditProfileViewModel
    {
        public string Id { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100, ErrorMessage = "First Name cannot be longer than 100 characters")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last Name cannot be longer than 100 characters")]
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        [Display(Name = "Profile Image URL")]
        public string? ProfileImageUrl { get; set; }

        public bool IsActive { get; set; }

        [Display(Name = "Theme Preference")]
        public string ThemePreference { get; set; } = "light";
       /* public List<SelectListItem> ThemeOptions { get; } = new List<SelectListItem>
       {
        new SelectListItem { Value = "light", Text = "Light" },
        new SelectListItem { Value = "dark", Text = "Dark" }
       };*/

        public List<SelectListItem>? ThemeOptions { get; set; }

        public List<string> SelectedRoles = new();
        public List<SelectListItem> AvaliableRoles = new();

    }
}
