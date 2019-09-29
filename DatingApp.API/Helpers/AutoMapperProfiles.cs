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
            // Usuários

            CreateMap<User, UserForListDto>() // <TSource, TDestination>
                .ForMember(destination => destination.PhotoUrl, options =>
                {
                    options.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url);  // obtive a foto principal e retornei url dela
                })
                .ForMember(destination => destination.Age, options =>
                {
                    options.MapFrom(src => src.DateOfBirth.CalculateAge());
                });

            CreateMap<User, UserForDetailedDto>()
                .ForMember(destination => destination.PhotoUrl, options => {
                    options.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url);
                })
                .ForMember(destination => destination.Age, options =>
                {
                    options.MapFrom(src => src.DateOfBirth.CalculateAge());
                });

            CreateMap<UserForUpdateDto, User>();

            CreateMap<UserForRegisterDto, User>();


            // Fotos

            CreateMap<Photo, PhotosForDetailedDto>();

            CreateMap<Photo, PhotoForReturnDto>();

            CreateMap<PhotoForCreationDto, Photo>();



        }

    }

}
