using Booklir.Models;

namespace Booklir.Core.Results
{
    public class AdminStatistics
    {
        public int TotalTrips { get; set; }
        public int TotalBookings { get; set; }
        public int TotalUsers { get; set; }
        public List<BookingActivity> RecentBookings { get; set; }
        public ChartData BookingTrends { get; set; }

    }
}
