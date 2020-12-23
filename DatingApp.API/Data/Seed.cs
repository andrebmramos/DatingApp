using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
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

        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if (!userManager.Users.Any()) // Fará seed somente se não houver usuário nenhum cadastrado
            {
                // Ler e desserializar arquivo de exemplo
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                // Criar Roles
                var roles = new List<Role>
                {
                    new Role { Name = "Member" },
                    new Role { Name = "Admin" },
                    new Role { Name = "Moderator" },
                    new Role { Name = "VIP" },
                };
                foreach (var role in roles)
                {
                    roleManager.CreateAsync(role).Wait(); // ...Wait pq o método é async, mas estamos numa função sync
                }

                // Criar usuários
                foreach (var user in users)
                {
                    userManager.CreateAsync(user, "pass").Wait();
                    userManager.AddToRoleAsync(user, "Member").Wait(); // Todos são membros
                }

                // Criar admin
                var adminUser = new User  {  UserName = "admin"  };
                var result = userManager.CreateAsync(adminUser, "pass").Result;
                if (result.Succeeded)
                {
                    var admin = userManager.FindByNameAsync("admin").Result;
                    userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
                }



                // context.SaveChanges(); // ao usar UserMan ager, não precisa disso, é automático
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
