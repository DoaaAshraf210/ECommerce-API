using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{

    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public AdminDashboardController(IDashboardService service) => _service = service;

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> Summary(CancellationToken ct)
            => Ok(await _service.GetSummaryAsync(ct));

        [HttpGet("sales")]
        public async Task<ActionResult<List<SalesPointDto>>> Sales(
            [FromQuery] int days = 30, CancellationToken ct = default)
            => Ok(await _service.GetSalesAsync(days, ct));


        [HttpGet("top-products")]
        public async Task<ActionResult<List<TopProductDto>>> TopProducts(
             [FromQuery] int days = 30, [FromQuery] int take = 5, CancellationToken ct = default)
             => Ok(await _service.GetTopProductsAsync(days, take, ct));

        [HttpGet("low-stock")]
        public async Task<ActionResult<List<LowStockProductDto>>> LowStock(
            [FromQuery] int threshold = 5, [FromQuery] int take = 20, CancellationToken ct = default)
            => Ok(await _service.GetLowStockAsync(threshold, take, ct));
    
    }
}