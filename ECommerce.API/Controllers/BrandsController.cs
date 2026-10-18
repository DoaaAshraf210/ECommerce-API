using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Brands;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _service;

        public BrandsController(IBrandService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<BrandDto>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BrandDto>> GetById(int id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]

        public async Task<ActionResult<BrandDto>> Create(CreateBrandDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]

        public async Task<IActionResult> Update(int id, UpdateBrandDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]

        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

























    }
}
