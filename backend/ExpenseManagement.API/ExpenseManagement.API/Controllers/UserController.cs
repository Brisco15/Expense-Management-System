using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;



namespace ExpenseManagement.API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("fixed")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false
            )
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 10)
            {
                return BadRequest("PageNumber must be >= 1, PageSize must be 1-10.");
            }

            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize, includeInactive);
            if(users == null || !users.Items.Any())
            {
                return Ok(new PagedResult<UserDto>
                {
                    Items = new List<UserDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = 0
                });
            }
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);

        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateUserRoleAsync(id, updateRoleDto.Role);
            if (!result)
            {
                return NotFound($"User with ID {id} not found or invalid role.");
            }
            return Ok(new { message = "Role updated successfully", userId = id });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult>UpdateStatus(int id, [FromBody] UpdateUserStatusDto updateUserStatusDto) 
        {
            var result = await _userService.UpdateUserStatusAsync(id, updateUserStatusDto.IsActive);
            if (!result)
            {
                return NotFound(new { error = $"User with ID {id} not found." });
            }

            return Ok(new
            {
                message = updateUserStatusDto.IsActive ? "User activated" : "User deactivated",
                userId = id
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
          
            if (id == currentUserId)
            {
                return BadRequest("You cannot delete your own account.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return NoContent();
        }
    }
}
