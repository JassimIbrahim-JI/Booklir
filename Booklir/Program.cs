using Booklir.Core.Interfaces;
using Booklir.Core.RazorHelper;
using Booklir.Core.Service;
using Booklir.Infrastructure.Data;
using Booklir.Infrastructure.Settings;
using Booklir.Mapping;
using Booklir.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add User-secrets 
builder.Configuration.AddUserSecrets<Program>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(BookingDestinationMappingProfile));
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddAutoMapper(typeof(TripMappingProfile));

//Connection String
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookingConnection")));


// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
}).AddEntityFrameworkStores<BookingDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BooklirCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Remember Me duartion

    //Cookies Setting
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BookingOwner", policy =>
        policy.RequireAssertion(context =>
        
    context.User.IsInRole("Admin") || context.Resource is Booking booking &&
    booking.UserId == context.User.FindFirstValue(ClaimTypes.NameIdentifier)));
});

// Configure Settings
builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register EmailSender and Renderers
builder.Services.AddSingleton<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
builder.Services.AddTransient<IEmailSender, EmailSender>();


//Services
builder.Services.AddScoped<IAuthService, AuthSerivce>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
