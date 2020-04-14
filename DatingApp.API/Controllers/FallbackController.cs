using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    public class FallbackController : Controller
        // Aula 179, preparando para produção / publicação
        // O que fizemos foi compilar o Angular para a pasta wwwroot e habilitar 
        // app.UseDefaultFiles() e app.UseStaticFiles() no Configure do Startup
        // Agora a API pode servir o app Angular, que pode ser acessado em
        // localhost:5000, mas é necessário criar esse novo controlador Fallback
        // por motivos de navegação, já que o roteador Angular não é "entendido"
        // pelo servidor da API

    {
        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}