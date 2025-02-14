﻿using EstudoDividas.Constants;
using EstudoDividas.Contracts;
using EstudoDividas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace EstudoDividas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {

        PaymentServices _paymentServices;

        public PaymentController(PaymentServices paymentServices)
        {
            _paymentServices = paymentServices;
        }


        // POST: api/Payment/pay_friend
        [Authorize(Roles = Roles.usuario)]
        [HttpPost("pay_friend")]
        public async Task<IActionResult> payFriend([FromBody] PayFriendRequestContract request)
        {

            var response = await _paymentServices.payFriend(request);

            if (response.status != "ok")
                return BadRequest(response);
            return Ok(response);
        }


        // POST: api/Payment/confirm
        [Authorize(Roles = Roles.usuario)]
        [HttpPost("confirm")]
        public async Task<IActionResult> confirmPayment([FromBody] ConfirmPaymentRequestContract request)
        {

            var response = await _paymentServices.confirmPayment(request);

            if (response.status != "ok")
                return BadRequest(response);
            return Ok(response);
        }


        // GET: api/Payment/history
        [Authorize(Roles = Roles.usuario)]
        [HttpGet("history")]
        public async Task<IActionResult> getPaymentHistory(string userPublicId, string userPrivateId)
        {
            var response = await _paymentServices.getPaymentHistory(userPublicId, userPrivateId);

            if (response.status != "ok")
                return BadRequest(response);
            return Ok(response);
        }
    }
}
