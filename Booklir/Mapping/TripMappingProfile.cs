using AutoMapper;
using Booklir.Items;
using Booklir.Models;
using Booklir.ViewModels.Gallery;
using Booklir.ViewModels.Trip;
using Booklir.ViewModels;

namespace Booklir.Mapping
{
    public class TripMappingProfile : Profile
    {
        public TripMappingProfile()
        {
            // Trip - TripViewModel Mappings
            CreateMap<Trip, TripViewModel>()
      .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.Description))
      .ForMember(dest => dest.SeatsLeft, opt => opt.MapFrom(src => src.AvailableSeats))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
      .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
      .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
      .ForMember(dest => dest.GalleryImages, opt => opt.MapFrom(src =>
          src.GalleryImages
              .OrderBy(g => g.IsPrimary)
              .Select(g => g.ImageUrl)
              .ToList()))
      .ForMember(dest => dest.Itinerary, opt => opt.MapFrom(src =>
          src.Itinerary.OrderBy(i => i.DayNumber)));


            CreateMap<CreateTripViewModel, Trip>()
          .ForMember(dest => dest.GalleryImages, opt => opt.Ignore())
          .ForMember(dest => dest.Itinerary, opt => opt.Ignore())
          .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Active")); ;


            CreateMap<Trip, UpdateTripViewModel>()
            .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.Itinerary, opt => opt.MapFrom(src => src.Itinerary))
            .ForMember(dest => dest.ExistingImages, opt => opt.MapFrom(src => src.GalleryImages))
            .ForMember(dest => dest.PrimaryImageId, opt => opt.MapFrom(src =>
                src.GalleryImages.FirstOrDefault(i => i.IsPrimary).Id));

            CreateMap<UpdateTripViewModel, Trip>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImagePath))
                .ForMember(dest => dest.GalleryImages, opt => opt.Ignore())
                .ForMember(dest => dest.Itinerary, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ItineraryItem, ItineraryItemViewModel>().ReverseMap();


            CreateMap<GalleryImage, GalleryImageViewModel>().ReverseMap();
            CreateMap<ItineraryItem, ItineraryItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

        }
    }
}
