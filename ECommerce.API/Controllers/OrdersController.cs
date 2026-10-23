using ECommerce.API.Extensions;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service) => _service = service;

        [HttpPost]
        public async Task<ActionResult<OrderDto>> Checkout(CheckoutDto dto)
        {
            var order = await _service.CheckoutAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
            => Ok(await _service.GetMyOrderAsync(User.GetUserId(), id));

       

        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult> CancelMyOrder(int id)
        {
            var order = await _service.CancelMyOrderAsync(User.GetUserId(), id);
            return Ok(order);
        }


        [HttpGet]
        public async Task<ActionResult> GetMyOrders([FromQuery] OrderQueryParams query)
        {
            var orders = await _service.GetMyOrdersAsync(User.GetUserId(), query);
            return Ok(orders);
        }




    }
}
