using Booklir.enums;
using Booklir.ViewModels.Trip;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.Booking
{
    public class CreateBookingViewModel
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Please select between 1 and 10 travelers")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Available Seats")]
        public int MaxSeats { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes (optional)")]
        public string? Notes { get; set; }

        public bool AlreadyBooked { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Credit;

        [ValidateNever]
        public TripViewModel? Trip { get; set; }
    }
}
