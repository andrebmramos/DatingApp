using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DatingApp.API.Controllers
{

    // Aula 74, Seção 8: Extending the API

    [ServiceFilter(typeof(LogUserActivity))] // Aula 135, toda vez que os métodos são usados o filtro será chamado atualizando LastActive
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
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams) // Atenção, o HttpGet não recebe informação em Body, preciso informar [FromQuery]
        {
            // [FromQuery] Quer dize que os parâmentros vem na linha de endereço. O ASP vai reconhecê-los.
            // Aparentemente o objeto userParams é criado (daí a importância dos valores padrão)
            // e as proprieddes encontradas serão atribuídas (independente do case)
            // exemplo: http://localhost:5000/api/users?PageNumber=2&pAGeSize=3


            // Obter id do usuário atual para eliminá-lo da lista de usuários retornador ("filter out")
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            userParams.UserId = currentUserId;

            // Obter usuário para obter gênero. Verifico se já foi especificado um gênero para filtragem no 
            // userParams que está vindo do SPA, senão defino o gênero oposto do usuário atual
            var userFromRepo = await _repo.GetUser(currentUserId);
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male"; 
            }            

            // Solicito a lista de usuários ao repositório. Filtragem será feita lá usando os userParams fornecidos
            var users = await _repo.GetUsers(userParams); // Obtenho usuários dentro de uma página (aula 140)
                // return Ok(users); // PROBLEMA! Isso retornará os objetos com SENHAS e outras coisas que não quero visíveis,
                                     // por isso mapearemos para um Dto só com aquilo que desejamos
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users); // Dto mais básico, para listagem de vários users

            // Incluo informações de paginaçãço na resposta usando extensão (ver Extensions.cs)
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            // Notar que informações de paginação estão no HEADER (inseridas acima) e os dados estão no BODY (abaixo)
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

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Checar se já tem curtida entre do "id" no "recipientId" 
            var like = await _repo.GetLike(id, recipientId);
            if (like != null)
                return BadRequest("Você já curtiu esse usuário");

            // Checar que recipient existe
            if (await _repo.GetUser(recipientId) == null)
                return NotFound();

            // Tudo ok, preparo like e adiciono
            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };
            _repo.Add<Like>(like);
            if (await _repo.SaveAll())
                return Ok();

            // Erro...
            return BadRequest("Falha ao curtir");

        }


        [HttpPost("{id}/dislike/{recipientId}")]
        public async Task<IActionResult> DisLikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Checar se já tem curtida entre do "id" no "recipientId" 
            var like = await _repo.GetLike(id, recipientId);
            if (like == null)
                return BadRequest("Você não tinha curtido esse usuário");


            _repo.Delete<Like>(like);

            if (await _repo.SaveAll())
                return Ok();

            // Erro...
            return BadRequest("Falha ao descurtir");

        }


        [HttpGet("{id}/alreadylike/{recipientId}")]
        public async Task<IActionResult> AlreadyLikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Checar se já tem curtida entre do "id" no "recipientId" 
            var like = await _repo.GetLike(id, recipientId);
            if (like != null)
                return Ok(true);
            else
                return Ok(false);
        }

    }

}
