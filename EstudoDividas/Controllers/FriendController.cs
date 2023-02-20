using EstudoDividas.Constants;
using EstudoDividas.Contracts;
using EstudoDividas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstudoDividas.Controllers
{
    [Route("api/friend")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        FriendServices _friendServices;
        public FriendController(FriendServices friendServices)
        {
            _friendServices = friendServices;
        }

        [Authorize(Roles = Roles.usuario)]
        [HttpPost("add")]
        public IActionResult addFriend([FromBody] AddFriendRequestContract request)
        {

            var response = _friendServices.addFriend(request);

            if (response.status != "ok")
                return BadRequest(response.message);
            return Ok(response.message);
        }

        [Authorize(Roles = Roles.usuario)]
        [HttpGet("get_list")]
        public IActionResult getFriends(string userPublicId, string mode)
        {
            var response = _friendServices.getFriends(userPublicId, mode);

            if (response.status != "ok")
                return BadRequest(response.message);
            return Ok(response);
        }
    }
}
