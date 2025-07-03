using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.Destination
{
    public class UpdateDestinationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        // Show existing image URL so we can display a preview in the form
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Replace Image (optional)")]
        public IFormFile? NewImage { get; set; }

        [Display(Name = "Active?")]
        public bool IsActive { get; set; }
    }
}
