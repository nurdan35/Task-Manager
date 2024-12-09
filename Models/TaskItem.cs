using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "todo"; // todo, doing, done

        [Required]
        public DateTime DueDate { get; set; }
        
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Tag { get; set; } = string.Empty; // GNOS-20, Tagging

        public bool IsFlagged { get; set; } 
        
        [MaxLength(50)]  
        public string AssignedTo { get; set; } = string.Empty; // Assigned to user for collaboration
        public DateTime? ReminderDate { get; set; }       // For notifications
        
        [MaxLength(20)]  
        public string Priority { get; set; } = "Medium";  // Task priority level (Low, Medium, High)

        [MaxLength(50)] 
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")] 
        public ApplicationUser? User { get; set; } 
        public int? BoardId { get; set; }  // Temporarily nullable (?)
        
        [ForeignKey("BoardId")]  
        public Board? Board { get; set; } 
        
        // Zaman Takibi
        public ICollection<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();
        
        

    }
}