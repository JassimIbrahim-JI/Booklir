using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Booklir.enums;

namespace Booklir.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Range(1,10)]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        public PaymentMethod Method { get; set; }
        
        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.UNPAID;

        /// <summary>
        /// If using Stripe, this can hold the PaymentIntent ID or Charge ID.
        /// </summary>
        [MaxLength(100)]
        public string? PaymentGatewayTransactionId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Trip Trip { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; }
        // Timestamp when delete it
        public DateTime? DeletedAt { get; set; }
        // who deleted it
        public string? DeletedByAdminId { get; set; }

        // Many Bookings - One User
        // Many Bookings - One Trip

    }
}
