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

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "todo"; // todo, doing, done

        public DateTime DueDate { get; set; }
        
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Tag { get; set; } = string.Empty; // GNOS-20, Tagging

        public bool IsFlagged { get; set; } // Önemli görevler için işaretleme

        [MaxLength(50)] 
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]  ////////////////// Yeni eklendi!
        public ApplicationUser? User { get; set; } // Görev sahibi kullanıcı
        
        [MaxLength(50)]  // Kullanıcının atanmış ID'si için
        public string AssignedTo { get; set; } = string.Empty; // Assigned to user for collaboration
        public DateTime? ReminderDate { get; set; }       // For notifications
        
        [MaxLength(20)]  // Öncelik seviyeleri için (örneğin: düşük, orta, yüksek)
        public string Priority { get; set; } = "Normal";  // Task priority level

        public int? BoardId { get; set; }  // Temporarily nullable (?)
        
        [ForeignKey("BoardId")]  ////////////////// Yeni eklendi!
        public Board? Board { get; set; } // Görevin bağlı olduğu board
        
        // Zaman Takibi
        public ICollection<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>(); // Görev üzerinde geçirilen zamanlar
    }
}