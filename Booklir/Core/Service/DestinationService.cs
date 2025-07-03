using Booklir.Core.Interfaces;
using Booklir.Core.Results;
using Booklir.extensions;
using Booklir.Infrastructure.Data;
using Booklir.Models;
using Booklir.Queries;
using Microsoft.EntityFrameworkCore;

namespace Booklir.Core.Service
{
    public class DestinationService : IDestinationService
    {
        private readonly BookingDbContext _dbContext;
        private readonly IImageStorageService _stroage;
        public DestinationService(BookingDbContext dbContext, IImageStorageService stroage)
        {
            _dbContext = dbContext;
            _stroage = stroage;
        }

        public async Task<bool> CreateAsync(Destination destination, IFormFile image)
        {
            try
            {
                if(image !=null && image.Length > 0)
                {
                    if(!IsValidImage(image))
                        throw new ArgumentException("Allowed formats: .png, .jpg, .jpeg, Max size: 8MB");

                    destination.ImageUrl = await _stroage.UploadImageAsync(image,"destinations");
                }
                await _dbContext.Destinations.AddAsync(destination);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidImage(IFormFile image)
        {
            var allowExtensions = new[] {".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(image.FileName).ToLower();
            return allowExtensions.Contains(fileExtension) && image.Length < 8 * 1024 * 1024;
        }


        public async Task<bool> ToogleActiveAsync(int id, bool status)
        {
            var destination = await GetByIdAsync(id);
            if (destination == null) return false;

            destination.IsActive = status;
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<Destination?> GetByIdAsync(int id)
        {
            return await _dbContext.Destinations
                    .Include(d => d.Trips.Where(t => !t.IsDeleted && t.Status == "Active"))
                    .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> UpdateAsync(Destination dto, IFormFile image)
        {
            try
            {
                var existing = await _dbContext.Destinations.FindAsync(dto.Id);
                if (existing == null) return false;

                if (image != null && image.Length > 0)
                {
                    if (!IsValidImage(image))
                        throw new ArgumentException("Allowed formats: .png, .jpg, .jpeg, Max size: 8MB");

                    var oldImage = existing.ImageUrl;
                    // Delete the old image if it exists

                    if (!string.IsNullOrEmpty(oldImage))
                    {
                        await _stroage.DeleteImageAsync(oldImage);
                    }

                    existing.ImageUrl = await _stroage.UploadImageAsync(image,"destinations");

                }
                existing.Name = dto.Name;
                existing.IsActive = dto.IsActive;

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IPagedResult<Destination>> GetAllAsync(DestiantionQueryParameters p)
        {
            var query = _dbContext.Destinations
                .Include(d=>d.Trips.Where(t=> !t.IsDeleted && t.Status == "Active"))
                .AsQueryable();

            if (!string.IsNullOrEmpty(p.Search))
                query = query.Where(d => d.Name.Contains(p.Search));

            if (p.Status)
                query = query.Where(d => d.IsActive);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((p.Page - 1) * p.PageSize)
                .Take(p.PageSize)
                .ToListAsync();

            return await query.OrderBy(b => b.Id).ToPagedResultAsync(p.Page, p.PageSize);
        }

        public async Task<IEnumerable<Destination>> GetPopularDestinationAsync(int count)
        {
           return await _dbContext.Destinations
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.Trips.Count)
                .Take(count)
                .ToListAsync();
        }
    }
}
