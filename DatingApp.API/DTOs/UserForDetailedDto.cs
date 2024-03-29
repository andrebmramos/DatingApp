﻿using DatingApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.DTOs
{
    public class UserForDetailedDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public int Age{ get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PhotoUrl { get; set; }
        // public ICollection<Photo> Photos { get; set; } // SEGURANÇA: A propriedade de navegação User dentro de Photo trará
                                                          // novamente infos de senha e salt que queremos esconder, então, no
                                                          // lugar da foto, traremos um DTO mapeado da mesma
        public ICollection<PhotosForDetailedDto> Photos { get; set; }

    }

}
