using Booklir.Items;
using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.Trip
{
    public class BookingTripViewModel
    {
        public int TripId { get; set; }

        [Display(Name = "Trip Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Price per person")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Seats Left")]
        public int SeatsLeft { get; set; }

        // Full URLs (or relative) for the carousel
        public List<string> GalleryImages { get; set; } = new();

        // Itinerary items (DayNumber, Title, Description)
        public List<ItineraryItem> Itinerary { get; set; } = new();

        // ---- Booking Fields (user fills these) ----

        [Required]
        [Display(Name = "Travel Date")]
        [DataType(DataType.Date)]
        // We’ll set a [Range]-like check in the controller/view so the user must pick between StartDate and EndDate.
        public DateTime TravelDate { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "You must book at least 1 traveler.")]
        [Display(Name = "Number of Travelers")]
        public int Travelers { get; set; } = 1;

        // Computed on the client side as Price * Travelers
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice => Price * Travelers;
    }
}
