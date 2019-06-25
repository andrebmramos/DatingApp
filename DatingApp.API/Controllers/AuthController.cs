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
    public class AuthController : ControllerBase 
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
            if (!ModelState.IsValid) {} /// bla bla bla

            userDto.Username = userDto.Username.ToLower();

            if (await _repo.UserExists(userDto.Username))
                return BadRequest("Usuário já existe");

            var userToCrete = new User { Username = userDto.Username };

            var createdUsre = await _repo.Register(userToCrete, userDto.Password);

            return StatusCode(201); // Provisório

        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userDto)  // Se naõ tivesse[ApíContoller], teria de acrescentar [FromBody] para ajudar compilador
        {
            var userFromRepo = await _repo.Login(userDto.Username.ToLower(), userDto.Password);

            if (userFromRepo == null)
                return Unauthorized(); // Sem mais informações!

            var claims = new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds

            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }

    }

}