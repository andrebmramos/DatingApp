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
            var resultContext = await next(); // Ou seja, ação concluída

            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var repo = (IDatingRepository)resultContext.HttpContext.RequestServices.GetService(typeof(IDatingRepository)); // Jeito de obter o repositório
            var user = await repo.GetUser(userId);
            user.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }

}
