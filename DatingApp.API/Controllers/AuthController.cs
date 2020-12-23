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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase // ControllerBase = Controller sem nada sobre View, que faremos em Angular
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IConfiguration config, IMapper mapper, 
                              UserManager<User> userManager, 
                              SignInManager<User> signInManager)
        {
            _config = config;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        #region Register antes de Identity
        //[HttpPost("register")]
        //public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto) // Cliente mandará nome e senha num objeto JSON no body da mensagem. Usaremos Dtos com esse propósito
        //                                                                                 // No Dto faremos validações com atributos [Required], etc.
        //                                                                                 // Se não tivesse[ApiContoller], teria de acrescentar [FromBody] para ajudar compilador
        //{
        //    // Validação por meio de [ApiController]
        //    // De modo que não preciso fazer
        //    // if (!ModelState.IsValid) {} /// bla bla bla

        //    userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

        //    if (await _repo.UserExists(userForRegisterDto.Username))
        //        return BadRequest("Usuário já existe");

        //    var userToCreate = _mapper.Map<User>(userForRegisterDto);  // new User { Username = userForRegisterDto.Username };

        //    var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

        //    // return StatusCode(201); // Provisório - depois, mudar para CreatedAtRoute(...), que indicará "um caminho" para obter o objeto criado

        //    var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser); // Mapeamento seguro (sem senhas) para retornar no CreatedAtRoute

        //    return CreatedAtRoute("GetUser", new { controller = "Users", id = createdUser.Id }, userToReturn);

        //} 
	    #endregion


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto) 
        {
            // Preliminares
            var userToCreate = _mapper.Map<User>(userForRegisterDto);         // preparo um User, que é requerido pela CreateAsync do UserManager
            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate); // Mapeamento seguro (sem senhas) para retornar no CreatedAtRoute
            
            // Criação pelo UserManager, verificação do resultado e retorno
            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);
            if (result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }
            return BadRequest(result.Errors);
        }


        #region Login Original, antes de Identity Framework
        //[HttpPost("login")]
        //public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)  // Se naõ tivesse[ApiContoller], teria de acrescentar [FromBody] para ajudar compilador
        //{
        //    // Linha de teste
        //    // throw new Exception("Computer says no!");

        //    // Uso repositório para logar e confiro imediatamente se obtive um usuário
        //    var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
        //    if (userFromRepo == null)
        //        return Unauthorized(); // Sem mais informações!


        //    // A PARTIR DAQUI, CRIAREMOS O TOKEN, QUE SERÁ RETORNADO PARA O CLIENTE
        //    // O cliente mandará esse token para o servidor nas próximas requisições, de modo
        //    // que o usuário será "mantido autenticado" pelo tempo de validade do token


        //    // Usuário foi obtido: quais são as Claims que eu quero no Token? Quero o id e o nome.
        //    var claims = new System.Security.Claims.Claim[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
        //        new Claim(ClaimTypes.Name, userFromRepo.UserName)
        //    };


        //    // Agora, preciso uma chave simétrica para assinar o token. 
        //    // "Metade" ("Key da Symmetric key") dessa chave estará no appsettings.json, (por isso injeto IConfiguration no construtor)
        //    SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
        //    SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);


        //    // "Descreve" o Token: 
        //    // Claims que eu defini, prazo de 1 dia, assinado pelas SigningCredentials criadas acima
        //    SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(claims),
        //        Expires = DateTime.Now.AddDays(1),
        //        SigningCredentials = creds
        //    };

        //    // Criação do Token
        //    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler(); // Classe que "cria, lê, etc. tokens"
        //    SecurityToken token = tokenHandler.CreateToken(tokenDescriptor); // Uso a classe acima para criar o token


        //    // Um objeto auxiliar que será retornado com dados do usuário.
        //    // Feito na aula 115 (Adding the main photo to the Nav bar), para que eu tenha
        //    // fácil acesso à foto (url) do usuário. Poderia,por exemplo, ter colocado no 
        //    // token, mas aí toda vez que mando token fico mandando dados a mais, o que não
        //    // seria crítico nesse caso, mas adotamos essa forma melhor de fazer
        //    var user = _mapper.Map<UserForListDto>(userFromRepo);

        //    // Token é retornado no cortpo do ObjectResult "Ok"
        //    return Ok(new
        //    {
        //        token = tokenHandler.WriteToken(token),
        //        user
        //    });

        //}
        #endregion

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)  // Se naõ tivesse[ApiContoller], teria de acrescentar [FromBody] para ajudar compilador
        {
            // Obter usuário pelo UserManager (se null, já retorna unauthorized)
            // Tentar login usando SignInManager com a senha fornecida e o usuário obtido
            // Se tive sucesso, 
            // ...mapear num usuário DTO (com informações simplificadas)
            // ...gerar um Token com as Claims que eu necessito (função auxiliar para token)
            // ...retornar Ok e o Token
            var userFromManager = await _userManager.FindByNameAsync(userForLoginDto.Username);
            if (userFromManager==null)
                return Unauthorized();
            var passResult = await _signInManager.CheckPasswordSignInAsync(userFromManager, userForLoginDto.Password, false);
            if (passResult.Succeeded)
            {
                var appUser = _mapper.Map<UserForListDto>(userFromManager);
                return Ok(new
                {
                    token = GenerateJWTToken(userFromManager).Result,
                    user = appUser
                });
            }
            return Unauthorized();
        }


        private async Task<string> GenerateJWTToken(User user)
        {
            // A PARTIR DAQUI, CRIAREMOS O TOKEN, QUE SERÁ RETORNADO PARA O CLIENTE
            // O cliente mandará esse token para o servidor nas próximas requisições, de modo
            // que o usuário será "mantido autenticado" pelo tempo de validade do token


            // Usuário foi obtido: quais são as Claims que eu quero no Token? Quero o id e o nome.
            var claims = new List<Claim> // Mudei de Claim[] para List<Claim> por conveniência, para usar Add adiante
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // Adicionando Claims dos Roles do usuário (aula 206)
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }



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

            return tokenHandler.WriteToken(token);

        }

    }

}