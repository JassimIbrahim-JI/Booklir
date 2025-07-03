using AutoMapper;
using Booklir.Models;
using Booklir.ViewModels.User;

namespace Booklir.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() 
        {
            // User - EditUserViewModel Mappings

            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom((src, dest, destNumber, context) =>
                context.Items["Roles"] as List<string> ?? new List<string>()));

            CreateMap<ApplicationUser, EditProfileViewModel>()
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl))
                .ForMember(dest => dest.ThemePreference, opt => opt.MapFrom(src => src.ThemePreference)) // Static data dropdown(light,dark)
                .ForMember(dest => dest.AvaliableRoles, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));


            // VM → User: ignore ProfileImageUrl because you set it manually after upload
            CreateMap<EditProfileViewModel, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore());

        }

    }
}
