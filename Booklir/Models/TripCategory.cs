using System.ComponentModel.DataAnnotations;

namespace Booklir.Models
{
    public class TripCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IconCss { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
