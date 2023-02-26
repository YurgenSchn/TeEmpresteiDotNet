using EstudoDividas.Contracts;
using Microsoft.AspNetCore.Mvc;
using EstudoDividas.Services;
using Microsoft.AspNetCore.Authorization;
using EstudoDividas.Constants;

namespace EstudoDividas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        // Trazer os serviços que serão utilizados pelos endpoints
        AuthenticationServices _authServices;

        public AuthenticationController(AuthenticationServices authServices)
        {
            _authServices = authServices;
        }

        // POST: api/Authentication/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserRequestContract request)
        {
            var response = _authServices.registerUser(request);
            if (response.status != "ok")
                return BadRequest(response);
            return Ok(response);
        }

        // POST: api/authentication/testtoken
        [Authorize(Roles = Roles.usuario)]
        [HttpPost("testToken")]
        public IActionResult testToken()
        {
            return Ok();
        }


        // POST: api/Authentication/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestContract request)
        {
            var response = _authServices.login(request);

            if (response.status != "ok")
                return BadRequest(response);
            return Ok(response);
        }

    }
}
