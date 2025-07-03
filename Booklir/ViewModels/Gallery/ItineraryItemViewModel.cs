using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.Gallery
{
    public class ItineraryItemViewModel
    {
        public int Id { get; set; }
        public int DayNumber { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        // Note: no TripId, no Trip navigation property here. 
        // We'll map the DayNumber/Title/Description back to the EF entity manually.
    }
}
