using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booklir.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Booking> Bookings { get; set; }
        public string ThemePreference { get; set; } = "light";
        public bool IsActive { get; set; }
        public string? ProfileImageUrl { get; set; }

        public DateTime? LastLoginDate { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // One User - Many Bookings
    }
}
