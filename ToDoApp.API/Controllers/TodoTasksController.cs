using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApp.Services.DTOs;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TodoTasksController : ControllerBase
{
    private readonly ITodoTaskService _todoTaskService;

    public TodoTasksController(ITodoTaskService todoTaskService)
    {
        _todoTaskService = todoTaskService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoTaskDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _todoTaskService.CreateAsync(dto, userId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllForUser(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isCompleted = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var tasks = await _todoTaskService.GetAllForUserAsync(userId, pageNumber, pageSize, searchTerm, categoryId, isCompleted);
        return Ok(tasks);
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> Update(Guid taskId, [FromBody] UpdateTodoTaskDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _todoTaskService.UpdateAsync(taskId, dto, userId);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> SoftDelete(Guid taskId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var deleted = await _todoTaskService.SoftDeleteAsync(taskId, userId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
