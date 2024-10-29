using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class Board
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty; // Foreign key for the user
        public ApplicationUser? User { get; set; } // Navigation property

        // Board'a iş birliği yapan kullanıcılar
        public ICollection<ApplicationUser> Collaborators { get; set; } = new List<ApplicationUser>(); // GNOS-23: Task sharing
    }
}