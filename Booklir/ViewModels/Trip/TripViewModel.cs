using System.ComponentModel.DataAnnotations;
using Booklir.Items;

namespace Booklir.ViewModels.Trip
{
    public class TripViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        [Display(Name = "Trip Description")]
        public string ShortDescription { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public List<string> GalleryImages { get; set; } = new();

        public string Status { get; set; } = string.Empty;

        public int SeatsLeft { get; set; }
        public bool IsFeatured { get; set; }


        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<ItineraryItem> Itinerary { get; set; } = new();
        public string PrimaryImage => GalleryImages.FirstOrDefault();

    }
}
