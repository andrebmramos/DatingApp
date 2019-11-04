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


            // Mensagens, aula 160

            CreateMap<MessageForCreationDto, Message>().ReverseMap(); // ReverseMap "aproveita" e já gera o mapa reverso

            CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, opt => opt.MapFrom(m => m.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, opt => opt.MapFrom(m => m.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));

        }

    }

}
