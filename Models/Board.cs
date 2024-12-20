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
        
        // BoardShare ilişkisi
        public ICollection<BoardShare> BoardShares { get; set; } = new List<BoardShare>();
    }
}