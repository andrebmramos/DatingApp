using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Models
{
    public class Like
    {
        // Notar que professor não criou LikeId

        // FKs
        public int LikerId { get; set; }
        public int LikeeId { get; set; }

        // Navegação
        public virtual User Liker { get; set; }
        public virtual User Likee { get; set; }

    }
}
