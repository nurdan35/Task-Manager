namespace TaskManagement.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // todo, doing, done
        public DateTime DueDate { get; set; }
        public string Tag { get; set; } // Etiketleme sistemi için
        public bool IsFlagged { get; set; } // Önemli görevler için işaretleme
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}

