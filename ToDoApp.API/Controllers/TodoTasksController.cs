using Microsoft.AspNetCore.Mvc;
using ToDoApp.DataAccess.Entities;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoTasksController : ControllerBase
{
    private readonly ITodoTaskService _todoTaskService;

    public TodoTasksController(ITodoTaskService todoTaskService)
    {
        _todoTaskService = todoTaskService;
    }

    [HttpPost]
    public Task<IActionResult> Create(TodoTask task)
    {
        throw new NotImplementedException();
    }

    [HttpGet("user/{userId}")]
    public Task<IActionResult> GetAllForUser(Guid userId)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{taskId}")]
    public Task<IActionResult> SoftDelete(Guid taskId)
    {
        throw new NotImplementedException();
    }
}
