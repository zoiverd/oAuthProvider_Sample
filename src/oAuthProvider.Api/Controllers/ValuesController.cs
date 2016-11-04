using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oAuthProvider.Api.Authorization;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace oAuthProvider.Api.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET: api/values
        [HttpGet]
        [Authorize(MyPolicies.Vendedor)]
        public string Get()
        {
            var login = HttpContext.User.GetClaim(ClaimTypes.NameIdentifier);
            var nome = HttpContext.User.GetClaim(ClaimTypes.GivenName);
            var email = HttpContext.User.GetClaim(ClaimTypes.GivenName);
            return $"Login: {login} - Nome: {nome} - Email: {email}";
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize(MyPolicies.Admin)]
        public string Get(int id)
        {
            return "Apenas admin";
        }
    }
}
