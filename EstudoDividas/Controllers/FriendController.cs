using EstudoDividas.Constants;
using EstudoDividas.Contracts;
using EstudoDividas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstudoDividas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        FriendServices _friendServices;
        public FriendController(FriendServices friendServices)
        {
            _friendServices = friendServices;
        }


        // POST: api/friend/add
        [Authorize(Roles = Roles.usuario)]
        [HttpPost("add")]
        public async Task<IActionResult> addFriend([FromBody] AddFriendRequestContract request)
        {

            var response = await _friendServices.addFriend(request);

            if (response.status != "ok")
                return BadRequest(response);
            return Ok(response);
        }


        // GET: api/friend/list
        [Authorize(Roles = Roles.usuario)]
        [HttpGet("list")]
        public async Task<IActionResult> getFriends(string userPublicId, string userPrivateId, string mode)
        {
            var response = await _friendServices.getFriends(userPublicId, userPrivateId, mode);

            if (response.status != "ok")
                return BadRequest(response);
            return Ok(response);
        }
    }
}
