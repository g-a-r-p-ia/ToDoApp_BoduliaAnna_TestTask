namespace ToDoApp.Services.DTOs;

public class TodoTaskDto
{
    public Guid Id { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid CategoryId { get; set; }
}