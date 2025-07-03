using System.ComponentModel.DataAnnotations;
using Booklir.Items;

namespace Booklir.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Invalid price")]
        public decimal Price { get; set; }

        [Display(Name = "Trip Image")]
        [Obsolete("Use GalleryImages instead")]
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsFeatured { get; set; }
        public string Status { get; set; }
        public int AvailableSeats { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; }
        // Timestamp when delete it
        public DateTime? DeletedAt { get; set; }
        // who deleted it
        public string? DeletedByAdminId { get; set; }


        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByAdminId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByAdminId { get; set; }

        public ICollection<Booking> Bookings { get; set; }

        // One Trip - Many Bookings

        // FK
        public int TripCategoryId { get; set; }
        public TripCategory TripCategory { get; set; } = null!;

        public int DestinationId { get; set; }
        public Destination Destination { get; set; }
        public List<GalleryImage> GalleryImages { get; set; } = new List<GalleryImage>();
        public List<ItineraryItem> Itinerary { get; set; } = new List<ItineraryItem>(); // JSON serialized list of itinerary items
    }
}
