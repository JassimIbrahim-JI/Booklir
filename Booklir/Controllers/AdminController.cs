using AutoMapper;
using Booklir.Core.Interfaces;
using Booklir.Core.Results;
using Booklir.enums;
using Booklir.Infrastructure.Data;
using Booklir.Items;
using Booklir.Models;
using Booklir.Queries;
using Booklir.ViewModels;
using Booklir.ViewModels.Admin;
using Booklir.ViewModels.Booking;
using Booklir.ViewModels.Destination;
using Booklir.ViewModels.Gallery;
using Booklir.ViewModels.Trip;
using Booklir.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Booklir.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ITripService _tripService;
        private readonly ICategoryService _svc;
        private readonly IBookingService _bookingService;
        private readonly IDestinationService _destinationService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INotificationService _notificationService;
        private readonly IImageStorageService _ima;
        private readonly IHttpContextAccessor _context;
        private readonly BookingDbContext _Dbcontext;

        public AdminController(ITripService tripService,ICategoryService svc, IMapper mapper, UserManager<ApplicationUser> userManager, IBookingService bookingService, INotificationService notificationService, IImageStorageService ima, RoleManager<IdentityRole> roleManager, IDestinationService destinationService, IHttpContextAccessor context, BookingDbContext dbcontext)
        {
            _tripService = tripService;
            _svc = svc;
            _mapper = mapper;
            _userManager = userManager;
            _bookingService = bookingService;
            _notificationService = notificationService;
            _ima = ima;
            _roleManager = roleManager;
            _destinationService = destinationService;
            _context = context;
            _Dbcontext = dbcontext;
        }


        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var adminStatistics =await _tripService.GetAdminStatisticsAsync();

            ViewBag.BookingLabels = adminStatistics.BookingTrends.Labels;
            ViewBag.BookingData = adminStatistics.BookingTrends.Data;

            return View(adminStatistics);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var statistics = await _tripService.GetAdminStatisticsAsync();
            return Json(new
            {
                totalTrips = statistics.TotalTrips,
                totalBookings = statistics.TotalBookings,
                totalUsers = statistics.TotalUsers,
                bookingChart = new
                {
                    labels = statistics.BookingTrends.Labels ?? new List<string>(),
                    data = statistics.BookingTrends.Data ?? new List<int>()
                }

            });
        }

       // Trip Management
        [HttpGet]
        public async Task<IActionResult> Trips(TripQueryParameters parameters)
        {
           var result = await _tripService.GetTripsAsync(parameters);
            var vm =new TripListViewModel
            {
                Trips = _mapper.Map<IEnumerable<TripViewModel>>(result.Items),
                TotalTrips = result.TotalCount,
                Parameters = parameters
            };
            return View(vm);
        }


        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult>Bulk(BulkActionModel model)
        {
            if (model.Ids == null || !model.Ids.Any())
            {
                return Json(new { success = false, message = "No items selected" });
            }

            var adminId = _userManager.GetUserId(User);
            foreach(var id in model.Ids)
            {
                if (model.Action == "Delete")
                    await _tripService.SoftDeleteAsync(id, adminId);

                else if (model.Action == "Restore")
                    await _tripService.RestoreAsync(id, adminId);

                else 
                    return Json(new { success = false, message = "Invalid bulk action" });

            }
            return Json(new { success = true });

        }


        [HttpGet]
        public async Task<IActionResult> CreateTrip()
        {
            var vm = new CreateTripViewModel
            {
                // Initialize any defaults
                DestinationOptions = new List<SelectListItem>(),
                GalleryImages = new List<IFormFile> { null, null, null },
                Itinerary = new List<ItineraryItemViewModel>
                    {
                        new ItineraryItemViewModel { DayNumber = 1, Title = "", Description = "" }
                    }
            };

            // Populate the Destination dropdown
            await PopulateDestinationAndCategoryOptions(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrip(CreateTripViewModel vm)
        {
            // Manually validate image count
            if (vm.GalleryImages == null || vm.GalleryImages.Count(f => f != null) != 3)
            {
                ModelState.AddModelError(nameof(vm.GalleryImages), "Exactly 3 images are required.");
            }

            if (vm.Itinerary == null || !vm.Itinerary.Any())
            {
                ModelState.AddModelError(nameof(vm.Itinerary), "At least one itinerary item is required.");
            }

            if (!ModelState.IsValid)
            {
                PrepareTripViewModelForRedisplay(vm);
                await PopulateDestinationAndCategoryOptions(vm);
                return View(vm);
            }

            try
            {
                var trip = _mapper.Map<Trip>(vm);
               
                trip.Itinerary = vm.Itinerary
            .Where(i => !string.IsNullOrWhiteSpace(i.Title)
                     && !string.IsNullOrWhiteSpace(i.Description))
            .Select(i => new ItineraryItem
            {
                DayNumber = i.DayNumber,
                Title = i.Title,
                Description = i.Description
                // TripId will be set when EF saves the parent Trip
            })
            .ToList();

                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var success = await _tripService.CreateTripAsync(trip, vm.GalleryImages,adminId);
                if (success)
                {
                    TempData["SuccessTrip"] = "Trip created successfully!";
                    return RedirectToAction(nameof(Trips));
                }

                ModelState.AddModelError(string.Empty, "Failed to create trip. Please check the input and try again.");
            }
            catch (Exception ex)
            {

                if (ex is ArgumentException)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                else
                {
                    var inner = ex.InnerException?.Message ?? ex.Message;
                    TempData["ErrorTrip"] = $"Unexpected error: {inner}";
                }
            }

            PrepareTripViewModelForRedisplay(vm);
            await PopulateDestinationAndCategoryOptions(vm);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateTrip(int id)
        {

            var trip = await _tripService.GetTripByIdAsync(id);
            if (trip == null) return NotFound("Trip not found");

            var vm = _mapper.Map<UpdateTripViewModel>(trip);
            vm.ExistingImages = trip.GalleryImages.Select(g => new GalleryImageViewModel
            {
                Id = g.Id,
                ImageUrl = g.ImageUrl,
                IsPrimary = g.IsPrimary
            }).ToList();

            await PopulateDestinationAndCategoryOptions(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTrip(UpdateTripViewModel vm)
        {
            // Initialize collections to avoid null reference issues
            vm.Itinerary = vm.Itinerary ?? new List<ItineraryItemViewModel>();
            vm.ExistingImages = vm.ExistingImages ?? new List<GalleryImageViewModel>();

            // Early image count validation
            if (vm.ExistingImages.Count + (vm.NewImages?.Count ?? 0) > 3)
            {
                ModelState.AddModelError("", "Maximum 3 images allowed");
                await PopulateDestinationAndCategoryOptions(vm);
                return View(vm);
            }

            if (!ModelState.IsValid)
            {
                await PopulateDestinationAndCategoryOptions(vm);
                return View(vm);
            }

            try
            {
                var existingTrip = await _Dbcontext.Trips
                    .Include(t => t.GalleryImages)
                    .Include(t => t.Itinerary)
                    .FirstOrDefaultAsync(t => t.Id == vm.Id);

                if (existingTrip == null) return NotFound();

                // Get current admin ID
                var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Handle soft delete status changes
                if (vm.IsDeleted != existingTrip.IsDeleted)
                {
                    existingTrip.IsDeleted = vm.IsDeleted;
                    existingTrip.DeletedAt = vm.IsDeleted ? DateTime.UtcNow : null;
                    existingTrip.DeletedByAdminId = vm.IsDeleted ? currentAdminId : null;

                    // Add status message based on action
                    TempData["StatusMessage"] = vm.IsDeleted
                        ? "Trip has been soft deleted"
                        : "Trip has been restored";
                }

                // Process primary image selection
                if (vm.PrimaryImageId.HasValue)
                {
                    foreach (var img in existingTrip.GalleryImages)
                    {
                        img.IsPrimary = (img.Id == vm.PrimaryImageId.Value);
                    }
                }

                // Update scalar properties with audit tracking
                _mapper.Map(vm, existingTrip);
                existingTrip.UpdatedAt = DateTime.UtcNow;
                existingTrip.UpdatedByAdminId = currentAdminId;

                // Track images to delete AFTER successful save
                var imagesToDelete = new List<GalleryImage>();

                // Handle new images
                if (vm.NewImages?.Count > 0)
                {
                    foreach (var file in vm.NewImages)
                    {
                        if (file?.Length > 0)
                        {
                            var imageUrl = await _ima.UploadImageAsync(file, "trips");
                            existingTrip.GalleryImages.Add(new GalleryImage
                            {
                                ImageUrl = imageUrl,
                                IsPrimary = false
                            });
                        }
                    }
                }

                // Handle itinerary updates
                foreach (var item in vm.Itinerary)
                {
                    var existingItem = existingTrip.Itinerary.FirstOrDefault(i => i.Id == item.Id);
                    if (existingItem != null)
                    {
                        _mapper.Map(item, existingItem);
                    }
                    else
                    {
                        existingTrip.Itinerary.Add(_mapper.Map<ItineraryItem>(item));
                    }
                }

                // Remove deleted itinerary items
                var itemIds = vm.Itinerary.Select(i => i.Id).ToList();
                foreach (var item in existingTrip.Itinerary.ToList())
                {
                    if (!itemIds.Contains(item.Id))
                    {
                        existingTrip.Itinerary.Remove(item);
                        _Dbcontext.Entry(item).State = EntityState.Deleted;
                    }
                }

                // Identify deleted images
                var imageIds = vm.ExistingImages.Select(i => i.Id).ToList();
                foreach (var image in existingTrip.GalleryImages.ToList())
                {
                    if (!imageIds.Contains(image.Id))
                    {
                        imagesToDelete.Add(image); // Mark for deletion AFTER save
                        existingTrip.GalleryImages.Remove(image);
                        _Dbcontext.Entry(image).State = EntityState.Deleted;
                    }
                }

                // Update with service
                var success = await _tripService.UpdateTripAsync(existingTrip);

                if (success)
                {
                    // Delete images AFTER successful update
                    foreach (var image in imagesToDelete)
                    {
                        await _ima.DeleteImageAsync(image.ImageUrl);
                    }

                    TempData["SuccessMessage"] = "Trip updated successfully!";
                    return RedirectToAction(nameof(Trips));
                }

                TempData["ErrorMessage"] = "Failed to update trip";
                await PopulateDestinationAndCategoryOptions(vm);
                return View(vm);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ModelState.AddModelError("", "The record was modified by another user. Please refresh and try again.");
                await PopulateDestinationAndCategoryOptions(vm);
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating trip: {ex.Message}";
                await PopulateDestinationAndCategoryOptions(vm);
                return View(vm);
            }
        }
        private async Task PopulateDestinationAndCategoryOptions(dynamic viewModel)
        {
            var categories = await _svc.GetAllAsync();
            viewModel.CategoryOptions = categories
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToList();

            var destinations = await _destinationService.GetAllAsync(new DestiantionQueryParameters { PageSize = int.MaxValue });
            viewModel.DestinationOptions = destinations.Items
                .Where(d => d.IsActive)
                .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
                .ToList();
        }
        private void PrepareTripViewModelForRedisplay(CreateTripViewModel vm)
        {
            // Ensure exactly 3 images (pad with nulls)
            if (vm.GalleryImages == null)
                vm.GalleryImages = new List<IFormFile>();

            while (vm.GalleryImages.Count < 3)
                vm.GalleryImages.Add(null);

            // Ensure at least one itinerary item
            if (vm.Itinerary == null || !vm.Itinerary.Any())
            {
                vm.Itinerary = new List<ItineraryItemViewModel>
        {
            new ItineraryItemViewModel { DayNumber = 1, Title = "", Description = "" }
        };
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSoftTrip(int id)
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid Trip Id" });

            var adminId = _userManager.GetUserId(User);
            var result = await _tripService.SoftDeleteAsync(id,adminId);
            if (result)
                return Json(new { success = true, message = "Trip Deleted Successfully" });

            return Json(new { success = false, message = "Failed to delete trip" });
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Invalid Trip Id" });

                var result = await _tripService.DeleteTripAsync(id);
                if (result)
                    return Json(new { success = true, message = "Trip permanently deleted." });

            return Json(new { success = false, message = "Failed to permanently delete trip." });
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TripRestore(int id)
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid Trip Id" });

            var adminId = _userManager.GetUserId(User);
            var success = await _tripService.RestoreAsync(id,adminId);
            if (success)
            {
                var msg = "Trip Restored Successfully";
                return Json(new { success = true, message = msg });
            }
            return Json(new { success = false, message = "Failed to restore trip" });
        }

        [HttpGet]
        public async Task<IActionResult> ExportTrip(TripQueryParameters p) 
        {
            var csvStream = await _tripService.ExportCsvAsync(p);
            return File( csvStream,"text/csv", "trips.csv");
        }


        // Category Management

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var model = await _svc.GetAllAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateCategory()
     => View(new TripCategory());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(TripCategory model)
        {
            if (!ModelState.IsValid) return View(model);
            await _svc.CreateAsync(model);
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var cat = await _svc.GetByIdAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(TripCategory model)
        {
            if (!ModelState.IsValid) return View(model);
            await _svc.UpdateAsync(model);
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _svc.DeleteAsync(id);
            return RedirectToAction(nameof(Categories));
        }


        // User Management
        public async Task<IActionResult> Users(UserQueryParameters p)
        {
            var query =  _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(p.Search))
                query = query.Where(u => u.FirstName.Contains(p.Search) || u.LastName.Contains(p.Search) || u.Email.Contains(p.Search));

            if(p.IsActive.HasValue)
                query = query.Where(u=>u.IsActive == p.IsActive);

            var totalCount = await query.CountAsync();
            var users = await query.OrderBy(u=>u.LastName)
                .Skip((p.PageNumber -1) * p.PageSize)
                .Take(p.PageSize)
                .ToListAsync();

            var userRoles = new Dictionary<string, List<string>>();
            foreach(var user in users) 
            {
             var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            var vm = new UserListViewModel
            {
                Items = _mapper.Map<IEnumerable<UserViewModel>>(users,opt => opt.Items["Roles"] = userRoles),
                Parameters = p,
                TotalCount = totalCount
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound("User not found");
              var user  = await _userManager.Users
                .Include(u=>u.Bookings)
                .ThenInclude(b=>b.Trip)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
          {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound("User not found");
               
            var vm = _mapper.Map<EditProfileViewModel>(user);

            vm.ThemeOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "light", Text = "Light" },
                new SelectListItem { Value = "dark", Text = "Dark" }
            };

            vm.AvaliableRoles = (await _roleManager.Roles.ToListAsync())
             .Select(r => new SelectListItem {
                 Value = r.Name,
                 Text = r.Name,
                 Selected = _userManager.IsInRoleAsync(user, r.Name).Result
             }).ToList();

             return View(vm);
          }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound("User not found");

                    if (!ModelState.IsValid)
                    {
                    model.ThemeOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "light", Text = "Light" },
                        new SelectListItem { Value = "dark", Text = "Dark" }
                    };
                model.AvaliableRoles = await GetUserRolesSelectList(user);
                        return View(model);
                    }

                try
                {
                    if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                    {
                        var imageUrl = await _ima.UploadImageAsync(model.ProfileImage,"profile");
                        user.ProfileImageUrl = imageUrl;
                    }
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.IsActive = model.IsActive;
                    user.ThemePreference = model.ThemePreference;


                    var currentRole = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user,currentRole);
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "User updated successfully!";
                        return RedirectToAction(nameof(Users));
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
                    ModelState.AddModelError("", "An error occurred while updating the user. Please try again.");
                }
                    model.ThemeOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "light", Text = "Light" },
                        new SelectListItem { Value = "dark", Text = "Dark" }
                    };

            model.AvaliableRoles = await GetUserRolesSelectList(user);
            TempData["ErrorMessage"] = "Failed to update user.";
            return View(model);
        }

        private async Task<List<SelectListItem>>GetUserRolesSelectList(ApplicationUser user)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var userRole = await _userManager.GetRolesAsync(user); 

            return roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = userRole.Contains(r.Name)
            }).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId) 
        {
          var current = await _userManager.GetUserAsync(User);
            if (current.Id == userId) return Json(new { success = false, message = "You cannot delete your own account" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Json(new { success = false, message = "User not found" });
            var result = await _userManager.DeleteAsync(user);
             return Json(new {success = result.Succeeded, message = result.Succeeded ? "User deleted successfully": "Failed to delete user" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest("Invalid User ID");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsActive = !user.IsActive;

            if(user.IsActive)
            {
               user.LastLoginDate = DateTime.UtcNow; // Update last login date
            }

            await _userManager.UpdateAsync(user);

            return Json(new { success = true, isActive = user.IsActive, userId = user.Id, lastLogin = user.LastLoginDate?.ToString("g") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");
            await _userManager.AddToRoleAsync(user, role);
            return Json(new {success = true });
        }


        [HttpPost]
        public async Task<IActionResult> BulkUserActions(string action, List<string> userIds)
        {
            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) continue;

                switch (action)
                {
                    case "activate":
                        user.IsActive = true;
                        break;
                    case "deactivate":
                        user.IsActive = false;
                        break;
                    case "delete":
                        await _userManager.DeleteAsync(user);
                        break;
                }
                await _userManager.UpdateAsync(user);
            }
            return Json(new { success = true });
        }


        // Booking Management
        [HttpGet]
        public async Task<IActionResult> Bookings(BookingQueryParameters p)
        {
            IPagedResult<Booking> paged;

            if (p.ShowDeleted)
            {
                // ignore the global filter to fetch deleted items
                paged = await _bookingService.GetBookingAsync(p, includeDeleted: true);
            }
            else
            {
                paged = await _bookingService.GetBookingAsync(p, includeDeleted: false);
            }

            var vm = new BookingListViewModel
            {
                Items = _mapper.Map<IEnumerable<BookingViewModel>>(paged.Items),
                TotalCount = paged.TotalCount,
                Parameters = p
            };
            return View(vm);
        }


        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkBookings(BulkActionModel model)
        {
            if (model.Ids == null || !model.Ids.Any())
            {
                return Json(new { success = false, message = "No bookings selected" });
            }

            var adminId = _userManager.GetUserId(User);
            if (adminId == null) return Json(new { success = false, message = "Admin not found" });

            foreach (var id in model.Ids)
            {
                switch (model.Action)
                {
                    case "Cancel" : await _bookingService.CancelBooking(id,adminId);
                        break;
                    case "Restore":
                        await _bookingService.RestoreBookingAsync(id, adminId);
                        break;
                    case "Confirm": await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Confirmed);
                        break;
                    case "Refund": await _bookingService.RefundAsync(id);
                        break;
                    case "MarkPaid":
                        await _bookingService.MarkPaymentStatusAsync(id, PaymentStatus.PAID);
                        break;
                    case "HardDelete":
                        await _bookingService.HardDeleteBookingAsync(id);
                        break;
                }
            }
            return Json(new { success = true, message = "Bulk action applied successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var adminId = _userManager.GetUserId(User);
            if (adminId == null)
                return Json(new { success = false, message = "Admin not found." });
            if (id <= 0)
                return Json(new { success = false, message = "Invalid booking ID." });

            var ok = await _bookingService.CancelBooking(id, adminId);
            return Json(new
            {
                success = ok,
                message = ok ? "Booking cancelled successfully." : "Failed to cancel booking."
            });
        }


        [HttpPost]
        public async Task<IActionResult> HardDeleteBooking(int id)
        {
            var ok = await _bookingService.HardDeleteBookingAsync(id);
            return Json(new
            {
                success = ok,
                message = ok ? "Booking deleted permanently." : "Failed to permanently delete booking."
            });
        }


        [HttpPost]
        public async Task<IActionResult> RestoreBooking(int id)
        {
            var adminId = _userManager.GetUserId(User);
            if (adminId == null)
                return Json(new { success = false, message = "Admin not found." });

            var ok = await _bookingService.RestoreBookingAsync(id, adminId);
            return Json(new
            {
                success = ok,
                message = ok ? "Booking restored." : "Failed to restore booking."
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, BookingStatus status)
        {
            var ok = await _bookingService.UpdateBookingStatusAsync(bookingId, status);
            return Json(new
            {
                success = ok,
                message = ok ? $"Status updated to {status}." : "Failed to update status."
            });
        }


        [HttpGet]
       public async Task<IActionResult> DownloadBookingInvoice(int bookingId) 
        {
            var pdf = await _bookingService.GenerateInvoicePdfAsync(bookingId);
            return File(pdf, "application/pdf", $"Invoice_{bookingId}.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> MarkCashPaid(int bookingId)
        {
            var booking = await _bookingService.GetBookingById(bookingId);
            if (booking == null)
                return Json(new { success = false, message = "Booking not found." });

            booking.PaymentStatus = PaymentStatus.PAID;
            booking.Status = BookingStatus.Confirmed;
            await _Dbcontext.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Payment marked as paid and booking confirmed."
            });
        }



        // Destination Management
        [HttpGet]
        public async Task<IActionResult>Destinations(DestiantionQueryParameters p)
        {
            var paged = await _destinationService.GetAllAsync(p);
            return View(paged);
        }

        [HttpGet] public IActionResult CreateDestination() => View(new CreateDestinationViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDestination(CreateDestinationViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Map VM → entity. ImageUrl will be assigned in the service after upload.
            var dest = _mapper.Map<Destination>(vm);

            var ok = await _destinationService.CreateAsync(dest,vm.Image);
            if (ok)
            {
                TempData["SuccessMessage"] = "Destination created successfully!";
                return RedirectToAction(nameof(Destinations));

            }
            TempData["ErrorMessage"] = "Failed to create destination.";
            return View(dest);
        }

        [HttpGet] public async Task<IActionResult> UpdateDestination(int id)
        {
            var destination = await _destinationService.GetByIdAsync(id);
            if (destination == null) return NotFound("Destination not found");

            // Map entity → VM so we can pre‐populate form fields
            var vm = _mapper.Map<UpdateDestinationViewModel>(destination);

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult>UpdateDestination(UpdateDestinationViewModel dto)
        {

            if(!ModelState.IsValid) return View(dto);

            // Map VM → entity. ImageUrl will be assigned in the service after upload.
            var dest = _mapper.Map<Destination>(dto);

            var ok = await _destinationService.UpdateAsync(dest, dto.NewImage);
            if (ok)
            {
                TempData["SuccessMessage"] = "Destination updated successfully!";
                return RedirectToAction(nameof(Destinations));
            }
            TempData["ErrorMessage"] = "Failed to update destination.";
            return View(dto);

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DestinationToggleActive(int id, bool status)
        {
            var ok = await _destinationService.ToogleActiveAsync(id, status);
            if (ok)
            {
                var msg = status ? "Destination activated successfully!" : "Destination deactivated successfully!";
                  return Json(new { success = true, message = msg });
            }
            return Json(new { success = false, message = "Could not update status" });
        }

        // Setting Management

        [HttpPost]
        public async Task<IActionResult> UpdateThemePreference(string theme)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });
            user.ThemePreference = theme;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notification = await _notificationService.GetRecentNotifactionsAsync();
            return PartialView("~/Views/Shared/Components/_NotificationPartial.cshtml", notification);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _notificationService.MarkAsReadAsync(notificationId);
            return Json(new { success = notification != null });
        }
    }


}
