using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class UserParams
    {
        // Parâmetros de paginação, aulas 141, 142
        private const int MaxPageSize = 50; // Importante limitar... vai que usuário pede 100 mil...

        public int PageNumber { get; set; } = 1; // Já temos um default p/página a ser visualizada...

        private int pageSize = 10; // ... e para o tamanho padrão
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }

        // Parâmetros de filtragem, aula 143
        public int UserId { get; set; }    // excluirei usuário do resultado (para ele não ver a si mesmo)

        public string Gender { get; set; } // mostrarei inicialmente usuários do gênero oposto

        public int MinAge { get; set; } = 18;

        public int MaxAge { get; set; } = 99;

        public string OrderBy { get; set; }

    }

}
