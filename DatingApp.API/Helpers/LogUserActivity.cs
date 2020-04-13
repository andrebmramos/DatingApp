using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter  // AULA 135. Action Filter para atualizar LastActive. 
                                                       // ...registrar serviço em startup.cs
                                                       // ...adicionar por atributo nos controllers: [ServiceFilter(typeof(LogUserActivity))]
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Context (não usamos) permite atuar durante execução da ação; Next permite atuar ao término da execução (aguardar)
            var resultContext = await next(); 

            // Apenas para monitoramento
            // Console.WriteLine($"$$$$$ concluída ação no Controller: {resultContext.Controller}, Rota: {resultContext.ActionDescriptor.DisplayName}");

            // Obter o Id (extraído da Claim que está no User que está no HttpContext que pode ser obtido do resultContext)
            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // Obter o repositório, mais ou menos pelo mesmo caminho (resultContext nos dá o HttpContext que permite buscar o serviço)
            var repo = (IDatingRepository)resultContext.HttpContext.RequestServices.GetService(typeof(IDatingRepository));
            // Tendo repositório e id, obtenho o usuário
            var user = await repo.GetUser(userId);
            // Ajusto o campo LastActive
            user.LastActive = DateTime.Now;
            // Salvo
            await repo.SaveAll();
        }

    }

}
