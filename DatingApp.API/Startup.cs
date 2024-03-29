﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;



using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DatingApp.API.Helpers;
using AutoMapper;
using Microsoft.Extensions.Hosting;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(ops => {
                ops.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
                ops.UseLazyLoadingProxies();
            });
            ConfigureServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            // MySQL
            // services.AddDbContext<DataContext>(ops =>
            // {
            //     ops.UseMySql(Configuration.GetConnectionString("MySQLConnection"));
            //     ops.UseLazyLoadingProxies();
            // });

            // SQL Server no Azure (configurar Connection Strings no App com Type SQLAzure)
            // services.AddDbContext<DataContext>(ops =>
            // {
            //     ops.UseSqlServer(Configuration.GetConnectionString("SQLServerConnectionAzure"));
            //     ops.UseLazyLoadingProxies();
            // });

            // SQL Server
            services.AddDbContext<DataContext>(ops =>
            {
                ops.UseSqlServer(Configuration.GetConnectionString("SQLServerConnection"));
                ops.UseLazyLoadingProxies();
            });

            ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Agora o contexto e provedor são ajustados nas funções acima, ConfigureDevelopmentServices e ConfigureProductionServices,
            // que são chamadas conforme selecionamos Prod. ou Dev. no arquivo launchSettings.json (pasta Properties). Ao fim dessas
            // funções, a execução passa para cá (ConfigureServices)
            // services.AddDbContext<DataContext>(ops => ops.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));


            services.AddControllers()
                .AddNewtonsoftJson(opt => 
                {
                    opt.SerializerSettings.ReferenceLoopHandling =
                            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            // Bloco como era no dotnet 2.2
            // services.AddMvc()
            //        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            //        .AddJsonOptions(opt =>
            //        {
            //            opt.SerializerSettings.ReferenceLoopHandling =
            //                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //        });  // Sobre AddJsonOptions: Aula 74, Seção 8: Extending the API, aproximadamente aos 8 
            //             // minutos explica que o fato de termos a propriedade de navegação user dentro da 
            //             // foto do user causa self referencing loop. A função GetUsers de UsersController
            //             // retorna "ok", porém o conteúdo vem com erro. Por isso fizemos essa configuração,
            //             // de modo ignorar a recorrência (VER ERRO NO TERMINAL da API)
            //             // Parece que haverá tratamento melhor disso adiante

            services.AddCors();

            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
                    // O conteúdo da seção em appsettings.json será refletido nos valores das propriedades declaradas na
                    // classe CloudinarySettings. Depois eu injeto no construtor de PhotosController da seguinte forma:   
                    // IOptions<CloudinarySettings> cloudinaryConfig
                    // ... e recupero os valores assim:
                    // cloudinaryConfig.Value.CloudName;    etc.

            services.AddAutoMapper(typeof(DatingRepository).Assembly);
                    // Preciso informar ao automapper em qual assembly estarão os profiles
                    // Faço isso "puxando" o tipo de qualquer classe que estará no Assembly
                    // (no caso, usei DatingRepository)

            services.AddTransient<Seed>();

            services.AddScoped<IAuthRepository, AuthRepository>();

            services.AddScoped<IDatingRepository, DatingRepository>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // Habilitar Authentication Middleware (Aula 34, sec. 3, Using the Authentication middleware)
                    .AddJwtBearer(options => 
                    {
                        options.TokenValidationParameters = new TokenValidationParameters 
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey( Encoding.ASCII
                                .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                            ValidateIssuer = false,  // localhost
                            ValidateAudience = false // localhost
                        };
                    });

            services.AddScoped<LogUserActivity>(); // Action Filter, será usado nos controllers (aula 135)
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env /*, Seed seeder*/)
        {
            if (env.IsDevelopment())
            {
                // Quando em Development mode, a linha abaixo faz a aplicação gerar uma
                // página de erro quando alguma exceção acontece
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Quando em Production mode, a página de erro não é legal para o usuário final.
                // Por outro lado, try catch em todos os lugares não é legal.
                // Então procedemos de outra forma criando um handler global para os erros
                // com algumas configurações convenientes

                // Comentado PROVISORIAMENTE na aula 191 ao publicar no Azure
                // Visto que não há problçemas, descomentamos novamente
                app.UseExceptionHandler(builder =>
                { // builder : IApplicationBuilder
                    builder.Run(async context =>     // Run é extension method, vem de Microsoft.AspNetCore.Builder
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // System.Net
                        var error = context.Features.Get<IExceptionHandlerFeature>(); // Microsoft.AspNetCore.Dignostics
                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message); // Microsoft.AspNetCore.Http (extension method WriteAsync)
                        }
                    });
                });

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(); // Descomentado na aula 191, ao publicar no Azure
            }

            app.UseHttpsRedirection(); // Descomentado na aula 191, ao publicar no Azure

            // Atenção: trazendo PROVISORIAMENTE a página de exceção para todos os casos na ocasião de implantação no Azure (aula 191)
            // pois é mais amigável para tratar exceções no  Azure. Visto que não há problçemas, comentamos novamente
            // app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseAuthentication(); // Controllers com atributo [Authorize] agora terão autenticação
            app.UseAuthorization();  // Necssário! ATENÇÃO PARA ORDEM! Primeiro autentica, depois autoriza          

            app.UseCors(action => action.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); // Política para "permitir recursos de localhost:4200 e 5000 conversarem"

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToController("Index", "Fallback");
            });

            // app.UseMvc(); // Obsoleto em dotnet 3, substituído por endpoints
        }
    }
}
