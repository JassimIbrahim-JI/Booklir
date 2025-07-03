using Booklir.Items;
using Booklir.ViewModels.Authentication;
using Booklir.ViewModels.Gallery;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.Trip
{
    public class CreateTripViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Trip Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [FutureDate(ErrorMessage = "Date must be in the future")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Gallery Images (3 required)")]
        [Required(ErrorMessage = "Please upload 3 images")]

        public List<IFormFile> GalleryImages { get; set; } = new();

        public List<ItineraryItemViewModel> Itinerary { get; set; } = new();

        [Range(1, 100)]
        public int AvailableSeats { get; set; }

        [FutureDate(ErrorMessage = "End date must be after start date")]
        public DateTime EndDate { get; set; }
        public bool IsFeatured { get; set; }
        public string Status { get; set; } = "Active"; // Default status


        [Display(Name = "Category")]
        [Required(ErrorMessage = "Please select a category")]
        public int TripCategoryId { get; set; }

        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }

        [Display(Name = "Destination")]
        [Required(ErrorMessage = "Please select a destination")]
        public int DestinationId { get; set; } 
       
        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> DestinationOptions { get; set; }
    }
}
