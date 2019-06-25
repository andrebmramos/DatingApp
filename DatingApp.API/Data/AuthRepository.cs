using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

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
            var user = await _context.Users.FirstOrDefaultAsync(us => us.Username==username);

            // Teste para usuário não encontrado
            if (user == null) 
                return null; // Controler entenderá null como erro 401 Unauthorized

            // Testa da senha
            if (VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)==false)
                return null;
            
            // Tudo válido!
            return user;     
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {            
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)) 
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
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512()) 
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(us => us.Username.Equals(username)))
                return true;
            else
                return false;
        }

    }

}