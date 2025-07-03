using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.Destination
{
    public class CreateDestinationViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Upload Image")]
        public IFormFile? Image { get; set; }

        [Display(Name = "Active?")]
        public bool IsActive { get; set; } = true;
    }
}
