using Booklir.Models;

namespace Booklir.Items
{
    public class ItineraryItem
    {
        public int Id { get; set; }
        public int DayNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }
    }
}
