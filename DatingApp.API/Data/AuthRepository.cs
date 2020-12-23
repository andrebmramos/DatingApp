using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;



// 
// 
// TODA FUNCIONALIDADE DESSA CLASSE REMOVIDA
// quando passamos a usar Identity. O que é feito aqui
// pass a ser assunido por classes como UserManager e SignInManager
// aula 204
//
//


namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {

        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }


        public async Task<User> Login(string username, string password)
        {
            // Estamos no contexto do repositório [AuthRepository]. O login no contexto
            // do controlador [AuthController] vai lidar com Tokens
            var user = await _context.Users
                .Include(u => u.Photos)
                .FirstOrDefaultAsync(us => us.UserName==username);

            // Teste para usuário não encontrado
            if (user == null) 
                return null; // Controller entenderá null como erro 401 Unauthorized

            // Testa da senha
            //if (VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)==false)
            //    return null; // sem detalhes!
            
            // Tudo válido!
            return user;     
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            // Inicializamos o HMACSHA512 com a semente aleatória que foi gerada no ato de registrar
            // o usuário
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)) 
            {                
                byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i=0; i< hash.Length; i++)
                    if (hash[i] != passwordHash[i])
                        return false; // Compara-se byte a byte, na primeira divergência detectada retorna false
            }
            return true; // Ok, hashs comparados com validade
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;

            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            //user.PasswordHash = passwordHash; // guardo o hash da senha
            //user.PasswordSalt = passwordSalt; // guardo a semente aleatória como salt

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // Usaremos HMACSHA512. Trata-se de um disposable que gera uma semente aleatória que usaremos como salt
            using (var hmac = new System.Security.Cryptography.HMACSHA512()) 
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(us => us.UserName.Equals(username)))
                return true;
            else
                return false;
        }

    }

}