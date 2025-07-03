using Booklir.Items;
using Booklir.Models;
using Booklir.ViewModels.Authentication;
using Booklir.ViewModels.Gallery;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels
{
    public class UpdateTripViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Trip Discription")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [FutureDate(ErrorMessage = "Start date must be in the future.")]
        public DateTime StartDate { get; set; }

        [FutureDate(ErrorMessage = "End date must be after the start date.")]
        public DateTime EndDate { get; set; }

        [Range(1,100)]
        public int AvailableSeats { get; set; }

        public bool IsFeatured { get; set; }
        public List<GalleryImageViewModel> ExistingImages { get; set; } = new(); // List of existing images for the trip
        public List<IFormFile> NewImages { get; set; } = new();
        public List<ItineraryItemViewModel> Itinerary { get; set; }
        public string? ImagePath { get; set; } // Path to the existing image
        public int? PrimaryImageId { get; set; }
        public bool IsDeleted { get; set; }
        public string Status { get; set; }


        [Display(Name = "Category")]
        public int TripCategoryId { get; set; }

        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }

        [Display(Name = "Destination")]
        public int DestinationId { get; set; } // Foreign key for the destination
       
        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> DestinationOptions { get; set; }

    }
}
