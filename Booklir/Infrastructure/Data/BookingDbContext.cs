using Booklir.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Booklir.Infrastructure.Data
{
    public class BookingDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {

        }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripCategory> TripCategories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Destination> Destinations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<Trip>()
         .Property(t => t.Price)
         .HasPrecision(18, 2);

            // Booking.TotalPrice as decimal(18,2)
        builder.Entity<Booking>()
            .Property(b => b.TotalPrice)
            .HasPrecision(18, 2);


            //Seed Roles when update migrations
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "role-admin",
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
               new IdentityRole
               {
                   Id = "role-user",
                   Name = "User",
                   NormalizedName = "USER"
               });

            //Seed Admin User
            var admin = new ApplicationUser
            {
                Id = "user-admin",
                UserName = "admin@booklir.com",
                NormalizedUserName = "ADMIN@BOOKLIR.COM",
                Email = "admin@booklir.com",
                NormalizedEmail = "ADMIN@BOOKLIR.COM",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,

                FirstName = "Booklir",
                LastName = "Administrator",
                ProfileImageUrl = null,
                ThemePreference = "light",
                
                IsActive = true
            };

            var haser = new PasswordHasher<ApplicationUser>();
            admin.PasswordHash = haser.HashPassword(admin, "Admin@1122");

            builder.Entity<ApplicationUser>().HasData(admin);


            //Assign admin user to Admin role
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                UserId = "user-admin",
                RoleId = "role-admin"
            });

            // Trip → ItineraryItem: configure cascade delete
            builder.Entity<Trip>()
                .HasMany(t => t.Itinerary)
                .WithOne(i => i.Trip)
                .HasForeignKey(i => i.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // If you also want GalleryImages to be removed automatically:
            builder.Entity<Trip>()
                .HasMany(t => t.GalleryImages)
                .WithOne(g => g.Trip)
                .HasForeignKey(g => g.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }
    }
}
