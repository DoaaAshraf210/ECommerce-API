using ECommerce.API.Extensions;
using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<CartDto>> Get()
            => Ok(await _service.GetCartAsync(User.GetUserId()));

        [HttpPost("items")]
        public async Task<ActionResult<CartDto>> AddItem(AddCartItemDto dto)
            => Ok(await _service.AddItemAsync(User.GetUserId(), dto));

        [HttpPut("items/{productId:int}")]
        public async Task<ActionResult<CartDto>> UpdateItem(int productId, UpdateCartItemDto dto)
            => Ok(await _service.UpdateItemAsync(User.GetUserId(), productId, dto));

        [HttpDelete("items/{productId:int}")]
        public async Task<ActionResult<CartDto>> RemoveItem(int productId)
            => Ok(await _service.RemoveItemAsync(User.GetUserId(), productId));

        [HttpDelete]
        public async Task<ActionResult> Clear()
        {
            await _service.ClearAsync(User.GetUserId());
            return NoContent();
        }
    }
}
