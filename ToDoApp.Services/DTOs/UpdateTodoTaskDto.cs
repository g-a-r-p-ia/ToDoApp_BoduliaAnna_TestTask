namespace ToDoApp.Services.DTOs;

public class UpdateTodoTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime? Deadline { get; set; }
}
