using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace ExpenseManagement.API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
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

            var result = await _userService.UpdateUserRoleAsync(id, updateRoleDto.Role);
            if (!result)
            {
                return NotFound($"User with ID {id} not found or invalid role.");
            }
            return Ok(new { Message = "Role updated successfully", userId = id });
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
                message = updateUserStatusDto.IsActive ? "User activated" : "User desactivated",
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
