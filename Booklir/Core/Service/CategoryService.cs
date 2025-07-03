using Booklir.Core.Interfaces;
using Booklir.Infrastructure.Data;
using Booklir.Models;
using Microsoft.EntityFrameworkCore;

namespace Booklir.Core.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly BookingDbContext _db;
        public CategoryService(BookingDbContext db)
        {
            _db = db;
        }

        public Task<IEnumerable<TripCategory>> GetAllAsync() =>
      _db.TripCategories.Where(c => c.IsActive).ToListAsync().ContinueWith(t => (IEnumerable<TripCategory>)t.Result);

        public Task<TripCategory?> GetByIdAsync(int id) =>
          _db.TripCategories.FindAsync(id).AsTask();

        public async Task CreateAsync(TripCategory e)
        {
            _db.TripCategories.Add(e);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(TripCategory e)
        {
            _db.Entry(e).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var c = await _db.TripCategories.FindAsync(id);
            if (c == null) throw new ArgumentException("Not found");
            c.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
