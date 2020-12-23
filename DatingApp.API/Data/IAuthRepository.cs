using System.Threading.Tasks;
using DatingApp.API.Models;

namespace DatingApp.API.Data 
{

    // 
    // 
    // TODA FUNCIONALIDADE DESSA CLASSE REMOVIDA
    // quando passamos a usar Identity. O que é feito aqui
    // pass a ser assunido por classes como UserManager e SignInManager
    // aula 204
    //
    //


    public interface IAuthRepository 
    {
        Task<User> Register(User user, string password);

        Task<User> Login(string username, string password);

        Task<bool> UserExists(string username);
        
    }

}