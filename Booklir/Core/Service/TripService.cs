using AutoMapper;
using Booklir.Core.Interfaces;
using Booklir.Core.Results;
using Booklir.enums;
using Booklir.extensions;
using Booklir.Infrastructure.Data;
using Booklir.Items;
using Booklir.Models;
using Booklir.Queries;
using Booklir.ViewModels.Booking;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Booklir.Core.Service
{
    public class TripService :ITripService
    {
        private readonly BookingDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IImageStorageService _storage;
        public TripService(BookingDbContext dbContext, IMapper mapper, IImageStorageService storage)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _storage = storage;
        }

        public async Task<bool> CreateTripAsync(Trip trip, List<IFormFile> imageFiles,string adminId)
        {
            try
            {

                var destinationExists = await _dbContext.Destinations
                .AnyAsync(d => d.Id == trip.DestinationId && d.IsActive);

                if (!destinationExists)
                {
                    throw new ArgumentException("Selected destination is invalid");
                }


                if (imageFiles == null || imageFiles.Count != 3)
                    throw new ArgumentException("Exactly 3 Gallery Images Required.");


                var galleryImages = new List<GalleryImage>();
                foreach (var file in imageFiles)
                {
                    if (!IsValidImage(file))
                        throw new ArgumentException("Invalid image format or size");

                    var imageUrl = await _storage.UploadImageAsync(file, "trips");
                    galleryImages.Add(new GalleryImage
                    {
                        ImageUrl = imageUrl,
                        IsPrimary = galleryImages.Count == 0
                    });
                }

                trip.GalleryImages = galleryImages;
                trip.CreatedByAdminId = adminId;
                trip.Status = "Active";

                if (!trip.Itinerary.Any())
                    throw new ArgumentException("At least one itinerary item required.");

                if (trip.StartDate >= trip.EndDate)
                    throw new ArgumentException("End-Date must be after Start-Date.");

                await _dbContext.Trips.AddAsync(trip);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                foreach (var image in trip.GalleryImages.Where(i => !string.IsNullOrEmpty(i.ImageUrl)))
                {
                    await _storage.DeleteImageAsync(image.ImageUrl);
                }
                throw;
            }
        }
        public async Task<bool> UpdateTripAsync(Trip existingTrip)
        {
            try
            {
                // Validate image count
                if (existingTrip.GalleryImages.Count > 3)
                    throw new ArgumentException("Maximum 3 images allowed");

                // Mark all modified entities
                _dbContext.Update(existingTrip);

                // Explicitly mark collections as modified
                foreach (var image in existingTrip.GalleryImages)
                {
                    _dbContext.Entry(image).State = image.Id == 0 ?
                        EntityState.Added : EntityState.Modified;
                }

                foreach (var item in existingTrip.Itinerary)
                {
                    _dbContext.Entry(item).State = item.Id == 0 ?
                        EntityState.Added : EntityState.Modified;
                }

                existingTrip.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> DeleteTripAsync(int tripId)
        {
                var trip = await _dbContext.Trips
                .Include(t=>t.Bookings)
                .FirstOrDefaultAsync(t=>t.Id == tripId);

                foreach(var booking in trip.Bookings)
                {
                // Option A: Remove each booking
                _dbContext.Bookings.Remove(booking); 

                // Option B: If you want to cancel bookings instead of deleting
                  //booking.Status = BookingStatus.Cancelled;
                }
               await _storage.DeleteImageAsync(trip.ImageUrl);

                _dbContext.Trips.Remove(trip);
                await _dbContext.SaveChangesAsync();
                return true;
            
          
        }
        public async Task<Trip?> GetTripByIdAsync(int tripId)
        {
            return await _dbContext.Trips
                .Include(t => t.GalleryImages)
                .Include(t => t.Itinerary)
                .Include(t => t.Destination)
                .FirstOrDefaultAsync(t => t.Id == tripId);
        }
        public async Task<IEnumerable<Trip>> GetAvailableTripsAsync()
        {
            return await _dbContext.Trips.Select(t => new Trip
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                ImageUrl = t.ImageUrl,
                AvailableSeats = t.AvailableSeats,
                Price = t.Price,
                // You can set the status based on  business logic
                Status = t.StartDate > DateTime.Now && t.AvailableSeats > 0 ? "Active" : "Inactive"
            }).ToListAsync();
        }
        public async Task<IEnumerable<Trip>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
          
            try
            {
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date must be less than or equal to end date.");
                }

                return await _dbContext.Trips
                    .Where(t => t.StartDate >= startDate && t.EndDate <= endDate) // Filter trips by date range
                    .ToListAsync(); 
               
            }
            catch (Exception)
            {
                return Enumerable.Empty<Trip>();
            }
        }
        public async Task<IEnumerable<Trip>> GetFeaturedTripsAsync()
        {
           return await _dbContext.Trips
                .Include(t=>t.GalleryImages)
                .Where(t => t.StartDate > DateTime.Now)
                .OrderBy(t=>t.StartDate)
                .Take(6)
                .ToListAsync(); 
        }
        public async Task<AdminStatistics> GetAdminStatisticsAsync()
        {
            // --- STEP 1: Fetch "raw" booking entities from the database
            var recentBookings = await _dbContext.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                .OrderByDescending(b => b.BookingDate)
                .Take(10)
                .ToListAsync();    //  executes SQL here

            // --- STEP 2: Project in‐memory using GetIconByStatus
            var bookings = recentBookings
                .Select(b => new BookingActivity
                {
                    UserName = b.User.UserName!,
                    TripTitle = b.Trip.Title,
                    BookingDate = b.BookingDate,
                    Status = b.Status.ToString(),
                    Description = "Booking made for " + b.Trip.Title,
                    Icon = GetIconByStatus(b.Status)   // now runs in memory
                })
                .ToList();

            // Monthly bookings grouping can stay in-database
            var rawMonthly = await _dbContext.Bookings
         .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
         .Select(g => new
         {
             Year = g.Key.Year,
             Month = g.Key.Month,
             Count = g.Count()
         })
         .ToListAsync();

            var monthlyBookings = rawMonthly
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x => new
                {
                    Label = $"{x.Year}-{x.Month:00}",   // C# string interpolation in memory
                    Count = x.Count
                })
                .ToList();

            return new AdminStatistics
            {
                TotalTrips = await _dbContext.Trips.CountAsync(),
                TotalUsers = await _dbContext.Users.CountAsync(),
                TotalBookings = await _dbContext.Bookings.CountAsync(),
                RecentBookings = bookings,
                BookingTrends = new ChartData
                {
                    Labels = monthlyBookings.Select(m => m.Label).ToList(),
                    Data = monthlyBookings.Select(m => m.Count).ToList()
                }
            };
        }

        private string GetIconByStatus(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Confirmed => "bi bi-check-circle",
                BookingStatus.Cancelled => "bi bi-x-circle",
                BookingStatus.Pending => "bi bi-clock",
                _ => "bi bi-question-circle"
            };
        }
        public async Task<bool> SoftDeleteAsync(int tripId, string adminId)
        {
            var trip = await _dbContext.Trips.FindAsync(tripId);
            if(trip == null) return false;
            trip.IsDeleted = true;
            trip.DeletedByAdminId = adminId;
            trip.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RestoreAsync(int tripId, string adminId)
        {
            var trip = await _dbContext.Trips.FindAsync(tripId);
            if(trip == null) return false;
            trip.IsDeleted = false;
            trip.UpdatedAt = DateTime.UtcNow;
            trip.UpdatedByAdminId = adminId;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<Stream> ExportCsvAsync(TripQueryParameters p)
        {
            var trips = await GetTripsAsync(p);
            var sb = new StringBuilder();
            sb.AppendLine("Id,Title,Description,StartDate,EndDate,AvailableSeats,Price,Status");
            foreach (var trip in trips.Items)
            {
                sb.AppendLine($"{trip.Id},{trip.Title},{trip.Description},{trip.StartDate},{trip.EndDate},{trip.AvailableSeats},{trip.Price},{trip.Status}");
            }

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            return stream;
        }
        public async Task<IPagedResult<Trip>> GetTripsAsync(TripQueryParameters p)
        {
            var query = _dbContext.Trips.AsQueryable();

            if (p.ShowDeleted == "true")
            {
                query = query.Where(t => t.IsDeleted);
            }
            else if (p.ShowDeleted == "false")
            {
                query = query.Where(t => !t.IsDeleted);
            }

            if (p.FromDate != null) query = query.Where(t => t.StartDate >= p.FromDate);
            if(p.ToDate != null) query = query.Where(t => t.EndDate <= p.ToDate);

            if (!string.IsNullOrWhiteSpace(p.SearchTerm))
            {
                query = query.Where(t => t.Title.Contains(p.SearchTerm) ||
                t.Description.Contains(p.SearchTerm));
            }

            if(p.Status == "Active" || p.Status == "Inactive")
            {
                query = query.Where(t => t.Status == p.Status);
            }

            return await query.OrderByDescending(t => t.StartDate).ToPagedResultAsync(p.PageNumber,p.PageSize);
        }
        public async Task<IPagedResult<Trip>> SearchHomeTripAsync(TripSearchFilters filter)
        {
            var query = _dbContext.Trips
                .Include(t=>t.Destination)
                .Include(t=>t.GalleryImages)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Query))
            {
                query = query.Where(t=>t.Title.Contains(filter.Query) || 
                                       t.Description.Contains(filter.Query) || 
                                       t.Destination.Name.Contains(filter.Query));
            }

            if (filter.MinPrice.HasValue)
                query = query.Where(t => t.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(t => t.Price <= filter.MaxPrice.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(t => t.StartDate >= filter.StartDate.Value);
         
            if (filter.EndDate.HasValue)
                query = query.Where(t => t.EndDate <= filter.EndDate.Value);
         
            if (filter.DestinationId.HasValue)
                query = query.Where(t => t.DestinationId == filter.DestinationId.Value);

            if (!string.IsNullOrEmpty(filter.Duration))
            {
                var (min, max) = filter.Duration switch
                {
                    "1-3" => (1, 3),
                    "4-7" => (4, 7),
                    "8+" => (8, int.MaxValue),
                    _ => (0, int.MaxValue)
                };
                query = query.Where(t =>
                    EF.Functions.DateDiffDay(t.StartDate, t.EndDate) >= min &&
                    EF.Functions.DateDiffDay(t.StartDate, t.EndDate) <= max);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(t => t.TripCategoryId == filter.CategoryId.Value);
            }

            // finally page & sort
            return await query
                .OrderBy(t => t.StartDate)
                .ToPagedResultAsync(filter.Page, filter.PageSize);

        }




        private bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            return allowedExtensions.Contains(extension) && file.Length <= 8 * 1024 * 1024;
        }
   }
}
