using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                .ForMember(destination => destination.PhotoUrl, options =>
                {
                    options.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url);
                })
                .ForMember(destination => destination.Age, options =>
                {
                    options.ResolveUsing(d => d.DateOfBirth.CalculateAge());
                });

            CreateMap<User, UserForDetailedDto>()
                .ForMember(destination => destination.PhotoUrl, options => {
                    options.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url);
                })
                .ForMember(destination => destination.Age, options =>
                {
                    options.ResolveUsing(d => d.DateOfBirth.CalculateAge());
                });

            CreateMap<Photo, PhotosForDetailedDto>();
        }

    }

}
