using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Models;
using System.Threading.Tasks;
using DatingApp.API.DTOs;

using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;
using AutoMapper;

namespace DatingApp.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase // ControllerBase = Controller sem nada sobre View, que faremos em Angular
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userDto) // Cliente mandará nome e senha num objeto JSON no body da mensagem. Usaremos Dtos com esse propósito
                                                                              // No Dto faremos validações com atributos [Required], etc.
                                                                              // Se naõ tivesse[ApiContoller], teria de acrescentar [FromBody] para ajudar compilador
        {
            // Validação por meio de [ApiController]
            // De modo que não preciso fazer
            // if (!ModelState.IsValid) {} /// bla bla bla

            userDto.Username = userDto.Username.ToLower();

            if (await _repo.UserExists(userDto.Username))
                return BadRequest("Usuário já existe");

            var userToCreate = new User { Username = userDto.Username };

            var createdUser = await _repo.Register(userToCreate, userDto.Password);

            return StatusCode(201); // Provisório - depois, mudar para CreatedAtRoute(...), que indicará "um caminho" para obter o objeto criado

        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)  // Se naõ tivesse[ApiContoller], teria de acrescentar [FromBody] para ajudar compilador
        {            
            // Linha de teste
            // throw new Exception("Computer says no!");

            // Uso repositório para logar e confiro imediatamente se obtive um usuário
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (userFromRepo == null)
                return Unauthorized(); // Sem mais informações!

            
            // A PARTIR DAQUI, CRIAREMOS O TOKEN, QUE SERÁ RETORNADO PARA O CLIENTE
            // O cliente mandará esse token para o servidor nas próximas requisições, de modo
            // que o usuário será "mantido autenticado" pelo tempo de validade do token


            // Usuário foi obtido: quais são as Claims que eu quero no Token? Quero o id e o nome.
            var claims = new System.Security.Claims.Claim[] 
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };


            // Agora, preciso uma chave simétrica para assinar o token. 
            // "Metade" ("Key da Symmetric key") dessa chave estará no appsettings.json, (por isso injeto IConfiguration no construtor)
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));            
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);


            // "Descreve" o Token: 
            // Claims que eu defini, prazo de 1 dia, assinado pelas SigningCredentials criadas acima
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            // Criação do Token
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler(); // Classe que "cria, lê, etc. tokens"
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor); // Uso a classe acima para criar o token


            // Um objeto auxiliar que será retornado com dados do usuário.
            // Feito na aula 115 (Adding the main photo to the Nav bar), para que eu tenha
            // fácil acesso à foto (url) do usuário. Poderia,por exemplo, ter colocado no 
            // token, mas aí toda vez que mando token fico mandando dados a mais, o que não
            // seria crítico nesse caso, mas adotamos essa forma melhor de fazer
            var user = _mapper.Map<UserForListDto>(userFromRepo);

            // Token é retornado no cortpo do ObjectResult "Ok"
            return Ok(new {
                token = tokenHandler.WriteToken(token),
                user
            });

        }            

    }

}