using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TaskManagement.Models
{
    public class Board
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty; // Foreign key for the user
        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; } // Navigation property

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<ApplicationUser> Collaborators { get; set; } = new List<ApplicationUser>(); // GNOS-23: Task sharing
    }
}