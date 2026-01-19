namespace todoApp.NET.Models;

public class ToDoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsComplete { get; set; }
    public DateTime? ComplitedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string Category { get; set; }
    public bool IsArchived { get; set; }
}