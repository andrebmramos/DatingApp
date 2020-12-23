using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            // AULA 105. Creating the Photos controller Part 1
            this._repo = repo;
            this._mapper = mapper;
            this._cloudinaryConfig = cloudinaryConfig;

            // O construtor foi injetado com interfaces, dentro dela, uma de opções que
            // é usada para obter as CloudinarySettings que, por sua vez, foram configuradas
            // no Startup.cs para serem carregadas do appsettings.json
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);

        }



        // Criando uma rota com nome que usarei posteriormente no método HttpPost
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }




        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto) 
        {
            // Atenção - foi necessário colocar [FROMFORM] para ajudar o controlador a localizar o elemento sendo passado
            // Verificar se Id confere como do usuário esperado
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Obtenho user do banco
            var userFromRepo = await _repo.GetUser(userId);

            // O próprio arquivo vem do Dto que está sendo passado
            var file = photoForCreationDto.File;

            // Auxiliar
            var uploadResult = new ImageUploadResult();

            // Testar se File tem conteúdo
            if (file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        // Arquivo: nome e conteúdo
                        File = new FileDescription(file.Name, stream),

                        // Transformação: formato quadrado, crop, focando na face
                        Transformation = new Transformation()
                            .Width(500)
                            .Height(500)
                            .Crop("fill")
                            .Gravity("face")

                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            // Ajusta últimos campos
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            // Finalmente...
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            // Se for a 1a foto, setar main
            if (!userFromRepo.Photos.Any(p => p.IsMain))
                photo.IsMain = true;

            userFromRepo.Photos.Add(photo);
            

            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo); // notar que a photo só tem id depois de salva no banco
                // return Ok(); // Preciso retornar uma rota para a própria foto, de modo poder mandar uma resposta
                                // útil para o cliente
                return CreatedAtRoute("GetPhoto", new { userId, id = photo.Id }, photoToReturn); // Complementado com userId ao migrar para dotnet 3
            }

            return BadRequest("Could not add the photo");

        }


        // Lembrar rota: // [Route("api/users/{userId}/photos")]
        // Vamos criar link tipo...http://localhost:5000/api/users/1/photos/121/setMain
        [HttpPost("{photoId}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int photoId)   // se renomear 1o campo para userIdx, não vai identificar 
        {
            // Como de costume, verifico se o usuário está correto
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _repo.GetUser(userId); // obtenho usuário

            // Agora verifico se o id da foto é o id de uma foto desse usuário
            if (!user.Photos.Any(p => p.Id == photoId))
                return Unauthorized();
            var photoFromRepo = await _repo.GetPhoto(photoId); // obtenho foto

            // Se foto já for main, "nada a fazer", retorna um BadRequest
            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo");

            // Faço alteração nas duas fotos
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;
            photoFromRepo.IsMain = true;
            if (await _repo.SaveAll())
                return NoContent(); // Sucesso

            // Se teve erro...
            return BadRequest("Could not set photo to main");

        }

        [HttpDelete("{photoId}")]
        public async Task<IActionResult> DeletePhoto(int userId, int photoId) // Aula 118
        {
            // Como de costume, verifico se o usuário está correto
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _repo.GetUser(userId); // obtenho usuário

            // Agora verifico se o id da foto é o id de uma foto desse usuário
            if (!user.Photos.Any(p => p.Id == photoId))
                return Unauthorized();
            var photoFromRepo = await _repo.GetPhoto(photoId); // obtenho foto

            // Se foto já for main, "nada a fazer", retorna um BadRequest
            if (photoFromRepo.IsMain)
                return BadRequest("You can't delete your main photo");


            // Temos fotos do Cloudinary e temos fotos do site que gera fotos aleatórias,
            // então vamos tratar separadamente. Par diferenciá-las, basta ver que só as do 
            // Cloudinary tem PublicId

            if (photoFromRepo.PublicId != null) 
            {
                // Excluir do Cloudinay
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);
                // Excluir do Banco
                if (result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            } else
            {
                // Só excluir do banco
                _repo.Delete(photoFromRepo);
            }

            // Salva e retorna Ok
            if (await _repo.SaveAll())
                return Ok();

            // Erro ao salvar, retorna erro
            return BadRequest("Failed to delete photo");
        }

    }

}
