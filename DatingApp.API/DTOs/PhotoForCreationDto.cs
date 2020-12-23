using Microsoft.AspNetCore.Http;
using System;

namespace DatingApp.API.DTOs
{
    public class PhotoForCreationDto
    {
        public string Url { get; set; }
        public IFormFile File { get; set; } // representa um arquivo enviado com a http request
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public string PublicId { get; set; }


        public PhotoForCreationDto()
        {
            this.DateAdded = DateTime.Now;
        }

    }
}
