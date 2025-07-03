using System.ComponentModel.DataAnnotations.Schema;

namespace Booklir.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // Booking // System // Trip // "User"
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RelatedUrl { get; set; } // URL to redirect when clicked
        
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } // Navigation property to User
    }
}
