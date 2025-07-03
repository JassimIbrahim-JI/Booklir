using AutoMapper;
using Booklir.Core.Interfaces;
using Booklir.Core.RazorHelper;
using Booklir.Infrastructure.Data;
using Booklir.Models;
using Booklir.ViewModels.Booking;
using Booklir.ViewModels.Trip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Booklir.Controllers
{
    public class BookingController : Controller
    {

        private readonly IBookingService _bookingService;
        private readonly ITripService _tripService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly BookingDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly IRazorViewToStringRenderer _razorRenderer;
        private readonly IConfiguration _config;

        public BookingController(IBookingService bookingService, ITripService tripService, IMapper mapper, UserManager<ApplicationUser> userManager, BookingDbContext dbContext, IEmailSender emailSender, IRazorViewToStringRenderer razorRenderer, IConfiguration config, INotificationService notificationService)
        {
            _bookingService = bookingService;
            _tripService = tripService;
            _mapper = mapper;
            _userManager = userManager;
            _dbContext = dbContext;
            _emailSender = emailSender;
            _razorRenderer = razorRenderer;
            _config = config;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var bookings = await _bookingService.GetUserBookingsAsync(user.Id);
            var vmList = _mapper.Map<IEnumerable<BookingViewModel>>(bookings);
            return View(vmList);
        }


        [Authorize, HttpGet]
        public async Task<IActionResult> Create(int tripId)
        {
            var trip = await _tripService.GetTripByIdAsync(tripId);
            if (trip == null)
            {
                TempData["ErrorCreateBooking"] = "Trip not found.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);

            bool alreadyBooked = await _dbContext.Bookings
           .AnyAsync(b => b.TripId == tripId && b.UserId == user.Id);

            var vm = new CreateBookingViewModel
            {
                TripId = trip.Id,
                Trip = _mapper.Map<TripViewModel>(trip),
                MaxSeats = trip.AvailableSeats,
                Quantity = 1,
                AlreadyBooked = alreadyBooked
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState
                                .Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            try
            {
                var (bookingId, clientSecret) =
                    await _bookingService.CreateBookingWithPaymentAsync(
                        model.TripId,
                        model.Quantity,
                        user.Id,
                        model.Notes,
                        model.PaymentMethod
                    );


                // 2) notify the booker (user)
                var userMessage = $"Your booking #{bookingId} for “{model.Trip.Title}” has been placed.";
                var userUrl = Url.Action("Confirmation", "Booking", new { id = bookingId });
                await _notificationService
                    .CreateNotificationAsync(user.Id, userMessage, userUrl);

                // 3) notify all admins (or swap for a single trip‐owner)
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var adminUrl = Url.Action("Details", "Admin", new { id = bookingId });
                var adminMessage = $"New booking #{bookingId} by {user.FullName} on “{model.Trip.Title}.”";

                foreach (var admin in admins)
                {
                    await _notificationService
                        .CreateNotificationAsync(admin.Id, adminMessage, adminUrl);
                }


                if (model.PaymentMethod == enums.PaymentMethod.Cash)
                {
                    return Json(new
                    {
                        success = true,
                        isPaymentRequired = false,
                        redirectUrl = Url.Action("Confirmation", new { id = bookingId })
                    });
                }

                return Json(new
                {
                    success = true,
                    isPaymentRequired = true,
                    clientSecret,
                    bookingId
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // log ex
                return Json(new
                {
                    success = false,
                    message = "An unexpected error occurred: " + ex.Message
                });
            }
        }


        [Authorize, HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {

           Debug.WriteLine($"Confirmation called with ID: {id}");

            var booking = await _bookingService.GetBookingById(id);
            Debug.WriteLine($"Booking found: {booking != null}");

            if (booking == null)
            {
                TempData["ErrorCreateBooking"] = "Booking not found.";
                return RedirectToAction("Index","Home");
            }

            var currentUserId = _userManager.GetUserId(User);
           Debug.WriteLine($"Current user ID: {currentUserId}, Booking user ID: {booking.UserId}");

            if (booking.UserId != currentUserId)
            {
                return Forbid();
            }

            var vm = _mapper.Map<BookingViewModel>(booking);
            return View(vm);
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _bookingService.GetBookingById(bookingId);

            // Verify booking ownership
            if (booking?.UserId != userId)
            {
                return Json(new
                {
                    success = false,
                    message = "Booking not found or access denied"
                });
            }

            var result = await _bookingService.CancelBooking(bookingId,userId);
            return Json(new
            {
                success = result,
                message = result ?
                    "Booking cancelled successfully" :
                    "Failed to cancel booking"
            });
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _bookingService.GetBookingById(id);

            if (booking?.UserId != userId)
                return Forbid();

            var stream = await _bookingService.GenerateInvoicePdfAsync(id);
            return File(stream, "application/pdf", $"Invoice_{id}.pdf");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Details(int bookingId)
        {
            // Fetch full booking details (including Trip data)
            var booking = await _bookingService.GetBookingById(bookingId);
            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }

            if (booking.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            // Pass the BookingViewModel directly to the Details view
            var vm = _mapper.Map<BookingViewModel>(booking);
            return View(vm);
        }

    }
}
