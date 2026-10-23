using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles =Roles.Admin)]
    public class AdminOrdersController : ControllerBase
    {
       
        private readonly IOrderService _service;

        public AdminOrdersController(IOrderService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetAll(
            [FromQuery] OrderQueryParams query)
            => Ok(await _service.GetAllAsync(query));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusDto dto)
            => Ok(await _service.UpdateStatusAsync(id, dto));

    }
}
