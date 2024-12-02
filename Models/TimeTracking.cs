using System.ComponentModel.DataAnnotations;
namespace TaskManagement.Models

{
    public class TimeTracking
    {
        public int TimeTrackingId { get; set; }

        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
        
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty; 
        public ApplicationUser User { get; set; } = new ApplicationUser();

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public TimeSpan Duration 
        {
            get 
            {
                return EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
            }
        }
    }
}