namespace ToDoApp.DataAccess.Entities;

public class TodoTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public Guid CategoryId { get; set; }
    public Guid UserId { get; set; }
     public DateTime? UpdatedAt { get; set; }
     public bool IsDeleted { get; set; } = false;

    public Category Category { get; set; } = null!;
    public User User { get; set; } = null!;
}
