using ECommerce.API.Extensions;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Users;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
   
        [ApiController]
        [Route("api/admin/users")]
        [Authorize(Roles = Roles.Admin)]
        public class AdminUsersController : ControllerBase
        {
            private readonly IUserAdminService _service;

            public AdminUsersController(IUserAdminService service) => _service = service;

            [HttpGet]
            public async Task<ActionResult<PagedResult<AdminUserListItemDto>>> GetAll(
                [FromQuery] UserQueryParams query)
                => Ok(await _service.GetUsersAsync(query));

            [HttpGet("{id:guid}")]
            public async Task<ActionResult<AdminUserDetailsDto>> GetById(Guid id)
                => Ok(await _service.GetUserAsync(id));

            [HttpPut("{id:guid}/lock")]
            public async Task<IActionResult> Lock(Guid id)
            {
                await _service.LockAsync(User.GetUserId(), id);
                return NoContent();
            }
        [HttpPut("{id:guid}/unlock")]
        public async Task<IActionResult> Unlock(Guid id)
        {
            await _service.UnlockAsync(User.GetUserId(), id);
            return NoContent();
        }

        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> SetRole(Guid id, [FromBody] SetUserRoleDto dto)
        {
            await _service.SetRoleAsync(User.GetUserId(), id, dto.Role);
            return NoContent();
        }
    }
    }
