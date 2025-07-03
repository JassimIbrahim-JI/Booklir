namespace Booklir.Models
{
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;


        // One-to-Many : One Destiantion has many trips
        public ICollection<Trip> Trips { get; set; }
    }
}
