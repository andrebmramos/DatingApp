using System;
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

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(ops => ops.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .AddJsonOptions(opt =>
                    {
                       opt.SerializerSettings.ReferenceLoopHandling =
                           Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    });  // Isso foi para evitar erro de referência por conta da
                         // propriedade de nagevação Photos
            services.AddCors();
            services.AddAutoMapper();
            services.AddTransient<Seed>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // Habilitar Authentication Middleware
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
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
                // Por outro lado, try cattch em todos os lugares não é legal.
                // Então procedemos de outra forma criando um handler global para os erros
                // com algumas configurações convenientes
                app.UseExceptionHandler(builder => { // builder : IApplicationBuilder
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
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            // seeder.SeedUsers(); // Rodar "só quando necessário"
            app.UseCors(action => action.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); // Política para "permitir recursos de localhost:4200 e 5000 conversarem"
            app.UseAuthentication(); // Controllers com atributo [Authorize] agora terão autenticação
            app.UseMvc(); // Framework que usamos
        }
    }
}
