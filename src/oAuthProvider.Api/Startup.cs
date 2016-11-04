using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using oAuthProvider.Api.Authorization;
using System.Text;

namespace oAuthProvider.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            // Add framework services.
            services
                .AddOptions()
                .AddAuthentication()
                .AddAuthorization(options => ConfigurePolicies(options))
                .AddCors()
                .AddMvc()
                .AddJsonOptions(x =>
                {
                    x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app
                //Manipula os erros para informar no retorno da api
                .UseExceptionHandler(errorApp => ErrorHandling(errorApp))
                .UseOAuthValidation()
                .UseOpenIdConnectServer(options =>
                {
                    //Passa a nossa implementação do provider de autorização
                    options.Provider = new AuthorizationProvider();
                    
                    //informa o endereço onde vai ficar o servidor de token
                    options.TokenEndpointPath = "/connect/token";

                    //TIRA ISTO NA PRODUCAO PELO AMOR DE DEUS
                    options.AllowInsecureHttp = true;

                    //Permite alterar o tempo de expiração dos tokens (1 hora por padrão) e refresh tokens (14 dias por padrão)
                    //options.AccessTokenLifetime;
                    //options.RefreshTokenLifetime;

                    //Também é necessário gerar uma chave e passar aqui para que você possa decriptar o token onde quiser
                    //Caso não passe, o .net vai pegar uma da máquina ou gerar uma automática (não lembro)
                    //E caso você troque de server, seus tokens não poderão ser decriptados
                    //options.SigningCredentials

                    //Tem mais uma penca de opções aqui, vale a pena uma estudada mais aprofundada
                    //mas caso seja uma autenticação simples de um sistema stand-alone, estas aqui serão suficientes
                })
                .UseMvc();

        }


        #region Configurar Autenticaçao

        private static void ConfigurePolicies(Microsoft.AspNetCore.Authorization.AuthorizationOptions options)
        {
            options.AddPolicy(MyPolicies.Admin, policy =>
            {
                //Aqui estou dizendo que apenas os usuarios com perfil admin (setado na criação do token) tem acesso a esta política
                policy.RequireClaim(MyClaims.Perfil, MyPolicies.Admin);
            });
            options.AddPolicy(MyPolicies.Vendedor, policy =>
            {
                //Aqui estou dizendo que para acessar a política de vendedor, o cara pode ser admin ou vendedor
                policy.RequireClaim(MyClaims.Perfil, MyPolicies.Admin, MyPolicies.Vendedor);
            });
        }

        #endregion

        #region Tratamento de erro

        private static void ErrorHandling(IApplicationBuilder errorApp)
        {
            errorApp.Run(async context =>
            {
                //Claro que não devemos voltar todas as exceptions com as descrições completas
                //Mas para teste está bom.
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var error = context.Features.Get<IExceptionHandlerFeature>();
                if (error != null)
                {
                    var ex = error.Error;
                    var innerEx = ex.InnerException;
                    dynamic serializedInner = null;

                    if (innerEx != null)
                    {
                        while (innerEx.InnerException != null)
                        {
                            innerEx = innerEx.InnerException;
                        }

                        serializedInner = new
                        {
                            message = innerEx.Message,
                            type = innerEx.GetType().Name
                        };
                    }

                    var serializedEx = new
                    {
                        message = ex.Message,
                        type = ex.GetType().Name,
                        innerEx = serializedInner
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(serializedEx), Encoding.UTF8);
                }
            });
        }
        #endregion
    }
}
