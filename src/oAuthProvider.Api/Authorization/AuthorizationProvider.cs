using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using static AspNet.Security.OpenIdConnect.Extensions.OpenIdConnectConstants;

namespace oAuthProvider.Api.Authorization
{
    public class AuthorizationProvider : OpenIdConnectServerProvider
    {
        public override Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            //Valida apenas os tipos de requisição de token
            //Se for colocar pra receber token de terceiros, tem que alterar aqui
            if (!context.Request.IsPasswordGrantType() && !context.Request.IsRefreshTokenGrantType())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Tipo de autenticaçao nao suportado pelo servidor.");

                return Task.FromResult(0);
            }

            //Mata o contexto caso não seja válido
            context.Skip();

            return Task.FromResult(0);
        }

        public override async Task HandleTokenRequest(HandleTokenRequestContext context)
        {
            var isValid = false;
            if (context.Request.IsPasswordGrantType())
            {
                //Caso o usuário informe usuário e senha na requisição do token, você deve validar se é válido
                isValid = context.Request.Username == "teste" && context.Request.Password == "teste";
            }
            else if (context.Request.IsRefreshTokenGrantType())
            {
                //Caso seja um refresh_token, você deve verificar se o usuário continua tendo acesso 
                //aos recursos e definir como válido ou não, note que não temos mais senha aqui
                //mas o token garante quem é o sujeito
                var login = context.Ticket.Principal.GetClaim(ClaimTypes.NameIdentifier);

                isValid = login == "teste";
            }

            if (!isValid)
            {
                //Aqui é a hora que define a requisição como inválida
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidGrant,
                    description: "Usuário/senha inválidos."
                );
                return;
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationScheme);

            //Aqui a gente cria os dados que queremos colocar no token
            //Como são dados de identificação não sensíveis, posso colocar no:
            //IdentityToken: Pode ser decriptado nos clients, serve para identificar em recursos externos
            //AccessToken: Token criptografado, que apenas o servidor que gerou (ou que possui a chave) possam decriptar as informações
            identity.AddClaim(ClaimTypes.Email, "teste@teste.com.br", Destinations.AccessToken, Destinations.IdentityToken);
            identity.AddClaim(ClaimTypes.GivenName, "Nome", Destinations.AccessToken, Destinations.IdentityToken);

            //Aqui são dados mais sensíveis, posso travar para enviar apenas no AccessToken
            identity.AddClaim(ClaimTypes.NameIdentifier, "teste", Destinations.AccessToken);
            identity.AddClaim(MyClaims.Perfil, MyPolicies.Vendedor, Destinations.AccessToken);

            //Coloca os dados no token
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                context.Options.AuthenticationScheme
            );

            //Define os escopos do token, esta informação pode ser recebida no request e revalidada para enviar apenas o que o client pediu
            //Caso não vá precisar autenticar em outros serviços (99% das vezes) você pode suprimir o OpenId
            //Caso não queira permitir o refresh da autorização sem senha, pode suprimir o OfflineAccess
            ticket.SetScopes(
                Scopes.OpenId,
                Scopes.OfflineAccess
            );

            //Você também pode informar os recursos a que este usuário tem acesso, isto será enviado no Identity Token, para o client travar um menu por ex....
            //ticket.SetResources("ApiCadastro, ApiCliente, ApiVenda, ...");

            context.Validate(ticket);

        }

    }
}
