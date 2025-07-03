using Booklir.Core.Results;
using Booklir.enums;
using Booklir.Models;
using Booklir.Queries;

namespace Booklir.Core.Interfaces
{
    public interface IBookingService
    {
        Task<Booking>GetBookingById (int bookingId);
        Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status);
        Task<int> ReserveSeats(int tripId, int numberOfSeats, string userId, string? note);
        Task<(int bookingId, string? paymentIntentClientSecret)> CreateBookingWithPaymentAsync(
    int tripId,
    int quantity,
    string userId,
    string? note,
    enums.PaymentMethod paymentMethod);
        Task<bool> CancelBooking(int bookingId, string adminId);
        Task<bool> HardDeleteBookingAsync(int id);
        Task<bool> RestoreBookingAsync(int id,string adminId);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
        Task<IPagedResult<Booking>> GetBookingAsync(BookingQueryParameters p, bool includeDeleted = false);
        Task<bool> RefundAsync(int bookingId);
        Task<Stream> GenerateInvoicePdfAsync(int bookingId);
        Task<bool> MarkPaymentStatusAsync(int bookingId, PaymentStatus newStatus);
    }
}
