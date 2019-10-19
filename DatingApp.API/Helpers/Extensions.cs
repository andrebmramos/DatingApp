using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {

        // Essa função auxiliar inclui detalhes de erro e elementos de CORS na
        // resposta http. O erro foi capturado e processado em outro pipeline,
        // então preciso colocar esses elementos de CORS no header para não dar
        // problema na página Angular 
        // (ok... rever aula 49. Setting up the Global exception handler in the API)
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }


        public static int CalculateAge(this DateTime date)
        {
            var age = DateTime.Today.Year - date.Year; // primeiro obtenho diferença de anos
            if (date.AddYears(age) > DateTime.Today)   // se data + anos > data atual é pq ainda não fez aniversário este ano ;-)
                age--;
            return age;
        }


        // Auxiliar para adicionar header com informações de paginação nas respostas http
        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            // response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader)); 
                // problema: por padrão, gera PascalCase (CurrentPage), e no Angular será aguardado camelCase (currentPage) 
                // resolveremos com um formatador:
            var camelCaseFormater = new JsonSerializerSettings();
            camelCaseFormater.ContractResolver = new CamelCasePropertyNamesContractResolver(); // Kkkkk, nome ridículo de longo
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormater));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination"); // para evitar erro de CORS
        }

    }
}
