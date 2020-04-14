using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Models
{
    public class Message
    {
        // Diferente de Likes, aqui foi criado um Id...
        public int Id { get; set; }

        // FKs
        public int SenderId { get; set; }
        public int RecipientId { get; set; }


        // Navigation
        public virtual User Sender { get; set; }
        public virtual User Recipient { get; set; }


        // Outras...
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }

    }
}
