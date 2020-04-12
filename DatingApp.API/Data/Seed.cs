using DatingApp.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class Seed
    {

        /*
        private readonly DataContext _context;

        public Seed(DataContext context)
        {
            _context = context;
        }
        */

        public static void SeedUsers(DataContext context)
        {
            if (!context.Users.Any()) // Fará seed somente se não houver usuário nenhum cadastrado
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);
                foreach (var user in users)
                {
                    byte[] hash, salt;
                    CreatePasswordHash("pass", out hash, out salt);
                    user.PasswordHash = hash;
                    user.PasswordSalt = salt;
                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
                }
                context.SaveChanges();
            }           
        }


        // Copiando método de geração de hash / salt do AuthRepository
        // (só para efeitos de desenvolvimento)
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

    }

}
