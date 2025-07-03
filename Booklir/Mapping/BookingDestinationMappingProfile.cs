using AutoMapper;
using Booklir.Items;
using Booklir.Models;
using Booklir.ViewModels;
using Booklir.ViewModels.Booking;
using Booklir.ViewModels.Destination;
using Booklir.ViewModels.Gallery;
using Booklir.ViewModels.Trip;
using Booklir.ViewModels.User;

namespace Booklir.Mapping
{
    public class BookingDestinationMappingProfile : Profile
    {
        public BookingDestinationMappingProfile() 
        {
          
            // Booking - BookingViewModel Mappings 
            CreateMap<Booking, BookingViewModel>()
                .ForMember(dest => dest.TripTitle, opt => opt.MapFrom(src => src.Trip.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.PrimaryImage, opt => opt.MapFrom(src =>
                    src.Trip.GalleryImages != null && src.Trip.GalleryImages.Any(i => i.IsPrimary)
                        ? src.Trip.GalleryImages.FirstOrDefault(i => i.IsPrimary).ImageUrl
                        : src.Trip.GalleryImages != null && src.Trip.GalleryImages.Any()
                            ? src.Trip.GalleryImages.First().ImageUrl
                            : string.Empty));


            // Create → Destination
            CreateMap<CreateDestinationViewModel, Destination>()
                // Destination.ImageUrl will be set manually in the service after upload
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                // We also ignore Trips (navigation property)
                .ForMember(dest => dest.Trips, opt => opt.Ignore());

            // Update → Destination
            CreateMap<UpdateDestinationViewModel, Destination>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Trips, opt => opt.Ignore());


            // Destination → UpdateDestinationViewModel (for pre‐populating the form)
            CreateMap<Destination, UpdateDestinationViewModel>()
                .ForMember(vm => vm.ExistingImageUrl, opt => opt.MapFrom(src => src.ImageUrl));

        }
    }
}
