using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DatingApp.API.Controllers
{

    // Aula 74, Seção 8: Extending the API

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]    
    public class UsersController : ControllerBase
    {

        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetUsers();
            // return Ok(users); // PROBLEMA! Isso retornará os objetos com SENHAS e outras coisas que não quero visíveis,
                                 // por isso mapearemos para um Dto só com aquilo que desejamos
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users); // Dto mais básico, para listagem de vários users
            return Ok(usersToReturn);
        }


        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user); // Dto mais detalhado
            return Ok(userToReturn);
        }
        

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            // AULA 99, Seção 10: Updating Resources

            // Verificar se Id confere como do usuário esperado
            // User é propriedade herdade de ControllerBase
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Obtenho user do banco
            var user = await _repo.GetUser(id);

            // Map
            _mapper.Map(userForUpdateDto, user);

            // Atualizo e retorno NoContent()
            if (await _repo.SaveAll())
                return NoContent();

            // Se houve problema, lanço erro
            throw new Exception($"Atualização do usuário {id} falhou no servidor");
        }

    }

}
