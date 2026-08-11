namespace ToDoApp.Services.DTOs;

public class CreateTodoTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public Guid CategoryId { get; set; } 
}