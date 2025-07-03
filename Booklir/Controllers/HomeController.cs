using AutoMapper;
using Booklir.Core.Interfaces;
using Booklir.Core.Service;
using Booklir.Models;
using Booklir.Queries;
using Booklir.ViewModels;
using Booklir.ViewModels.Trip;
using Booklir.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Booklir.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITripService _tripService;
        private readonly ICategoryService _categoryService;
        private readonly IDestinationService _destinationService;
        private readonly IMapper mapper;
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, ITripService tripService, IMapper mapper, IDestinationService destinationService, IConfiguration config, IEmailSender emailSender, ICategoryService categoryService)
        {
            _logger = logger;
            _tripService = tripService;
            this.mapper = mapper;
            _destinationService = destinationService;
            _config = config;
            _emailSender = emailSender;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var trips = await _tripService.GetFeaturedTripsAsync();
            var vm = mapper.Map<IEnumerable<TripViewModel>>(trips);
            
            Console.WriteLine($"Trips found: {trips.Count()}");

            var dest = await _destinationService.GetAllAsync(new DestiantionQueryParameters() {PageSize = int.MaxValue});
            var destDto = dest.Items.Where(d => d.IsActive)
                .Select(d => new Destination
                {
                    Id = d.Id,
                    Name = d.Name,
                    ImageUrl = d.ImageUrl,
                }).ToList();


            var categories = await _categoryService.GetAllAsync();
            var catVm = categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    IconCss = c.IconCss
                });

            var HomeVm =new HomeIndexViewModel
            {
                FeaturedTrips = vm,
                Destinations = destDto,
                Categories = catVm
            };

            return View(HomeVm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            if (trip == null)
            {
                return NotFound();
            }

            // Map Trip -> BookingTripViewModel
            var vm = new BookingTripViewModel
            {
                TripId = trip.Id,
                Title = trip.Title,
                Price = trip.Price,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                SeatsLeft = trip.AvailableSeats,
                GalleryImages = trip.GalleryImages
                                   .Select(img => img.ImageUrl)  
                                   .ToList(),
                Itinerary = trip.Itinerary.OrderBy(i => i.DayNumber).ToList(),

                // Default the TravelDate to StartDate
                TravelDate = trip.StartDate,

                // Default number of travelers to 1
                Travelers = 1
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Send email to admin
                var adminEmail = _config["EmailSettings:AdminEmail"];
                var subject = $"New Contact Message: {model.Subject}";

                var htmlMessage = $@"
                <h3>New Contact Message from Booklir Website</h3>
                <p><strong>Name:</strong> {model.FullName}</p>
                <p><strong>Email:</strong> {model.Email}</p>
                <p><strong>Phone:</strong> {model.PhoneNumber ?? "N/A"}</p>
                <p><strong>Preferred Contact Method:</strong> {model.PreferredMethod}</p>
                <hr>
                <p><strong>Message:</strong></p>
                <p>{model.Message}</p>
                <hr>
                <p><em>Received at: {DateTime.Now.ToString("f")}</em></p>
            ";

                await _emailSender.SendEmailAsync(adminEmail, subject, htmlMessage);

                // Send confirmation to user
                var userSubject = "Thank you for contacting Booklir";
                var userHtmlMessage = $@"
                <h3>Hello {model.FullName},</h3>
                <p>Thank you for reaching out to Booklir! We've received your message and our team will get back to you soon.</p>
                <p><strong>Your message:</strong></p>
                <blockquote>{model.Message}</blockquote>
                <p>We'll contact you via your preferred method: <strong>{model.PreferredMethod}</strong></p>
                <hr>
                <p>Best regards,<br>The Booklir Team</p>
            ";

                await _emailSender.SendEmailAsync(model.Email, userSubject, userHtmlMessage);

                TempData["SuccessMessage"] = "Your message has been sent successfully! We'll contact you soon.";
                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error sending contact message");

                ModelState.AddModelError("", "An error occurred while sending your message. Please try again later.");
                return View(model);
            }
        }

            [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubscribeNewsLetter(string email)
        {
            return Json(new
            {
                success = true,
                message = "Thanks for subscribing! 🎉"
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
