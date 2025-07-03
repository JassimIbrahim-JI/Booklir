using Booklir.Core.Interfaces;
using Booklir.Core.Results;
using Booklir.extensions;
using Booklir.Infrastructure.Data;
using Booklir.Models;
using Booklir.Queries;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using Booklir.enums;
using Stripe;
using SendGrid.Helpers.Mail;
using System;


namespace Booklir.Core.Service
{
    public class BookingService : IBookingService
    {
        private readonly BookingDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public BookingService(BookingDbContext dbContext, IWebHostEnvironment env, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _env = env;
            _notificationService = notificationService;
        }

        public async Task<bool> CancelBooking(int bookingId,string adminId)
        {
          var booking = await _dbContext.Bookings
                .Include(b=>b.Trip)
                .FirstOrDefaultAsync(b=>b.Id == bookingId);


            if (booking == null) return false;
            // restore seats
            booking.Trip.AvailableSeats += booking.Quantity; // Increase available seats 

            // mark cancelled + soft‐delete
            booking.Status = BookingStatus.Cancelled;
            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.UtcNow;
            booking.DeletedByAdminId = adminId; // Store the ID of the admin who cancelled

            //  _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HardDeleteBookingAsync(int id)
        {
            // bypass the query filter (because IsDeleted hides it)
            var b = await _dbContext.Bookings
                             .IgnoreQueryFilters()
                             .FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return false;

            _dbContext.Bookings.Remove(b);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreBookingAsync(int id,string adminId)
        {
            var b = await _dbContext.Bookings
                             .IgnoreQueryFilters()
                             .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted);
            if (b == null) return false;

            b.IsDeleted = false;
            b.DeletedAt = null;
            b.Status = BookingStatus.Pending; // or whatever default
            b.DeletedByAdminId = adminId;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Stream> GenerateInvoicePdfAsync(int bookingId)
        {
            // 1) Load booking (with Trip and User)
            var b = await _dbContext.Bookings
                .Include(bk => bk.Trip)
                .Include(bk => bk.User)
                .FirstOrDefaultAsync(bk => bk.Id == bookingId);

            if (b == null)
                throw new ArgumentException("Booking not found.");

            // 2) Create PDF document + page + graphics
            var doc = new PdfDocument();
            doc.Info.Title = $"Invoice_{bookingId}";
            var page = doc.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            // 3) Use a built-in PDF font (Helvetica) instead of Arial
        
            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);

            var normalFont = new XFont("Arial", 12, XFontStyle.Regular);

            // 4) Draw invoice details
            int y = 40;
            gfx.DrawString("Booking Invoice", titleFont, XBrushes.Black, 40, y);
            y += 40;

            gfx.DrawString($"Invoice Date: {DateTime.UtcNow:yyyy-MM-dd}", normalFont, XBrushes.Black, 40, y);
            y += 30;

            gfx.DrawString($"Booking ID: {b.Id}", normalFont, XBrushes.Black, 40, y);
            y += 20;

            gfx.DrawString($"User: {b.User.FullName} ({b.User.Email})", normalFont, XBrushes.Black, 40, y);
            y += 20;

            gfx.DrawString($"Trip: {b.Trip.Title}", normalFont, XBrushes.Black, 40, y);
            y += 20;

            gfx.DrawString($"Dates: {b.Trip.StartDate:yyyy-MM-dd} → {b.Trip.EndDate:yyyy-MM-dd}", normalFont, XBrushes.Black, 40, y);
            y += 20;

            gfx.DrawString($"Quantity: {b.Quantity}", normalFont, XBrushes.Black, 40, y);
            y += 20;

            decimal total = b.Trip.Price * b.Quantity;
            gfx.DrawString($"Total Paid: QAR {total:N2}", normalFont, XBrushes.Black, 40, y);
            y += 40;

            gfx.DrawString("Thank you for your booking!", normalFont, XBrushes.Black, 40, y);

            // 5) Save into a MemoryStream and return
            var stream = new MemoryStream();
            doc.Save(stream, false);
            stream.Position = 0;
            return stream;
        }

        public async Task<IPagedResult<Booking>> GetBookingAsync(BookingQueryParameters p, bool includeDeleted = false)
        {
            // choose whether to apply the global soft-delete filter
            IQueryable<Booking> query = includeDeleted
                ? _dbContext.Bookings.IgnoreQueryFilters()
                : _dbContext.Bookings;

            // eager-load user & trip
            query = query.Include(b => b.User)
                         .Include(b => b.Trip);

            if (p.FromDate.HasValue) 
                query = query.Where(b=>b.BookingDate >= p.FromDate.Value);
            
            if(p.ToDate.HasValue)
                query = query.Where(b=>b.BookingDate <= p.ToDate.Value);
            
            if (p.TripId.HasValue)
                query = query.Where(b => b.TripId == p.TripId.Value);
            
            if (!string.IsNullOrEmpty(p.UserEmail))
                query = query.Where(b => b.User.Email.Contains(p.UserEmail));

            if (p.Status.HasValue)
                query = query.Where(b => b.Status == p.Status.Value);

            if (!string.IsNullOrWhiteSpace(p.Search))
                query = query.Where(b => b.Trip.Title.Contains(p.Search) || b.User.FullName.Contains(p.Search));

            if (p.PaymentMethod.HasValue)
                query = query.Where(b => b.Method == p.PaymentMethod.Value);
            if (p.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == p.PaymentStatus.Value);

            // Order Newest Fiirst
            query = query.OrderByDescending(b => b.BookingDate);

            // if we're _not_ explicitly including deleted, the EF global filter
            // (HasQueryFilter(b => !b.IsDeleted)) will already hide them.
            // When includeDeleted==true, we bypass it so we can show them.

            // order & paginate
            query = query.OrderByDescending(b => b.BookingDate);
            return await query.ToPagedResultAsync(p.Page, p.PageSize);

        }

        public async Task<Booking> GetBookingById(int bookingId)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.GalleryImages) 
                .Include(b => b.User)
                .Where(b => b.Id == bookingId)
                .FirstOrDefaultAsync();

            return booking;
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
        {
            return await _dbContext.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.GalleryImages)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
        }

        public async Task<bool> RefundAsync(int bookingId)
        {
            var booking = await _dbContext.Bookings.Include(b => b.Trip).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return false;

            // Payement Gategway(Stripe or MyFatoorah) refund stub
            // var refundResult = await _payementGateway.RefundAsync(booking.PaymentTransctionId, booking.TotalPrice);
            // if(!refundResult.Success) return false;

            //booking.Status = BookingStatus.Refund;

            booking.Trip.AvailableSeats += booking.Quantity;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> ReserveSeats(int tripId, int numberOfSeats, string userId, string? note)
        {
            // Load the Trip (including GalleryImages if you need them later)
            var trip = await _dbContext.Trips
                .Include(t => t.GalleryImages)
                .FirstOrDefaultAsync(t => t.Id == tripId);


            if (trip == null)
                throw new ArgumentException("Trip not found");

            if (trip.AvailableSeats < numberOfSeats)
                throw new InvalidOperationException($"Only {trip.AvailableSeats} seats available");

            bool alreadyBooked = await _dbContext.Bookings
                .AnyAsync(b => b.TripId == tripId && b.UserId == userId);

            if (alreadyBooked)
            {
                throw new InvalidOperationException("You've already booked this trip");
            }



            // Create a new Booking entity
            var booking = new Booking
            {
                TripId = tripId,
                Quantity = numberOfSeats,
                UserId = userId,
                BookingDate = DateTime.UtcNow,
                Status = BookingStatus.Confirmed,
                Notes = note ?? string.Empty,
            };

            // Decrement available seats
            trip.AvailableSeats -= numberOfSeats;

            // Add and save in a try/catch
            _dbContext.Bookings.Add(booking);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbex)
            {
                // Re‐throw with the inner message so the controller can see it
                var inner = dbex.InnerException?.Message ?? dbex.Message;
                throw new Exception(inner, dbex);
            }
            return booking.Id;
        }

        public async Task<(int bookingId, string? paymentIntentClientSecret)> CreateBookingWithPaymentAsync(
    int tripId,
    int quantity,
    string userId,
    string? note,
    enums.PaymentMethod paymentMethod)
        {
            // 1) load & validate
            var trip = await _dbContext.Trips.FindAsync(tripId)
                       ?? throw new ArgumentException("Trip not found");
            if (trip.AvailableSeats < quantity)
                throw new InvalidOperationException($"Only {trip.AvailableSeats} seats available");
            if (await _dbContext.Bookings.AnyAsync(b => b.TripId == tripId && b.UserId == userId))
                throw new InvalidOperationException("You've already booked this trip");

            // 2) compute price & adjust seats
            decimal totalPrice = trip.Price * quantity;
            trip.AvailableSeats -= quantity;

            // 3) create booking entity
            var booking = new Booking
            {
                TripId = tripId,
                Quantity = quantity,
                UserId = userId,
                BookingDate = DateTime.UtcNow,
                Status = paymentMethod == enums.PaymentMethod.Cash ? BookingStatus.Confirmed : BookingStatus.Pending,
                Notes = note ?? string.Empty,
                Method = paymentMethod,
                PaymentStatus = paymentMethod == enums.PaymentMethod.Cash ? PaymentStatus.UNPAID : PaymentStatus.PENDING,
                TotalPrice = totalPrice
            };

            // 4) Stripe intent if needed
            string? clientSecret = null;
            if (paymentMethod == enums.PaymentMethod.Credit)
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(totalPrice * 100),
                    Currency = "sar",
                    Metadata = new Dictionary<string, string> { ["bookingId"] = "TBD" }
                };
                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);
                clientSecret = intent.ClientSecret;
                booking.PaymentGatewayTransactionId = intent.Id;
            }

            _dbContext.Bookings.Add(booking);

            // 5) save & catch DB errors
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // log dbEx.InnerException here
                throw;
            }

            // 6) best-effort: update Stripe metadata
            if (paymentMethod == enums.PaymentMethod.Credit)
            {
                try
                {
                    var svcUp = new PaymentIntentService();
                    await svcUp.UpdateAsync(
                      booking.PaymentGatewayTransactionId,
                      new PaymentIntentUpdateOptions { Metadata = new Dictionary<string, string> { ["bookingId"] = booking.Id.ToString() } }
                    );
                }
                catch { /* log but ignore */ }
            }

            // 7) best-effort: send notification
            try
            {
                await _notificationService.CreateNotificationAsync(
                  userId,
                  $"You booked: {trip.Title} for {quantity} person(s)",
                  null
                );
            }
            catch { /* ignore */ }

            return (booking.Id, clientSecret);
        }


        public async Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status)
        {
            var booking = await _dbContext.Bookings.Where(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (booking == null) return false;

            booking.Status = status;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Marks a booking’s PaymentStatus (and if it’s now Paid, mark BookingStatus=Confirmed).
        /// </summary>
        public async Task<bool> MarkPaymentStatusAsync(int bookingId, PaymentStatus newStatus)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return false;

            booking.PaymentStatus = newStatus;

            // If payment succeeded, also confirm the booking
            if (newStatus == PaymentStatus.PAID)
            {
                booking.Status = BookingStatus.Confirmed;
            }

            // If you want, you can handle PaymentStatus == Failed or Refunded here:
            //   - if Failed: restore seats, set BookingStatus = Cancelled, etc.
            //   - if Refunded: set BookingStatus = Refunded, refund seats, etc.

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
   
}
