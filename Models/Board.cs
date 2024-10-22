namespace TaskManagement.Models;

public class Board
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public string UserId { get; set; } = string.Empty;  // Foreign key for the user
    public ApplicationUser User { get; set; } = new ApplicationUser(); // Navigation property
}