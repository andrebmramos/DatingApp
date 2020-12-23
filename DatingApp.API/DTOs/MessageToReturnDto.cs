using System;

namespace DatingApp.API.Models
{
    public class MessageToReturnDto
    {
        // Diferente de Likes, aqui foi criado um Id...
        public int Id { get; set; }

        // Sender
        public int SenderId { get; set; }
        public string SenderKnownAs { get; set; }  // Automapper conseguirá identificar
        public string SenderPhotoUrl { get; set; }


        // Recipient
        public int RecipientId { get; set; }
        public string RecipientKnownAs { get; set; }  // Automapper conseguirá identificar
        public string RecipientPhotoUrl { get; set; }



        // Outras...
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        
    }
}
