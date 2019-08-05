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


namespace DatingApp.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase // ControllerBase = Controller sem nada sobre View, que faremos em Angular
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userDto)  // Se naõ tivesse[ApíContoller], teria de acrescentar [FromBody] para ajudar compilador
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
        public async Task<IActionResult> Login(UserForLoginDto userDto)  // Se naõ tivesse[ApiContoller], teria de acrescentar [FromBody] para ajudar compilador
        {            
            // Linha de teste
            // throw new Exception("Computer says no!");

            // Uso repositório para logar e confiro imediatamente se obtive um usuário
            var userFromRepo = await _repo.Login(userDto.Username.ToLower(), userDto.Password);
            if (userFromRepo == null)
                return Unauthorized(); // Sem mais informações!


            // Usuário foi obtido: quais são as Claims que eu quero no Token?
            var claims = new System.Security.Claims.Claim[] 
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };


            // O token será assinado com uma chave simétrica. 
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

            // Token é retornado no cortpo do ObjectResult "Ok"
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }            

    }

}