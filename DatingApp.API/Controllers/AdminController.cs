using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("userswithroles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {

            var users = await _context.Users
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Roles = (from userRole in u.UserRoles
                             join role in _context.Roles
                             on userRole.RoleId equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();
            // Exemplo de Json retornado:
            // {
            //    "id": 11,
            //    "userName": "admin",
            //    "roles": [
            //        "Admin",
            //        "Moderator"
            //    ]
            // }
            return Ok(users);
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            // pego o usuário
            var user = await _userManager.FindByNameAsync(userName);

            // pego as roles atuais dele
            var userRoles = await _userManager.GetRolesAsync(user);   

            // pego as roles que foram selecionadas
            var selectedRoles = roleEditDto.RoleNames;                // pego as roles que foram selecionadas
                // selectedRoles = selectedRoles ?? new string[] {};      // não entendi a necessidade; comenti, testei, continua ok 

            // Adiciona as Roles selecionadas que já não são roles do usuário
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded)
                return BadRequest("Falha ao adicionar Roles");

            // Remove as Roles ativas que não foram selecionadas
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)
                return BadRequest("Falha ao remover Roles");

            return Ok(await _userManager.GetRolesAsync(user));

        }



        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public IActionResult GetPhotosForModeration()
        {
            return Ok("Only moderators can see this (admin = moderator)");
        }

    }

}
