using Microsoft.AspNetCore.Mvc.Rendering;


namespace TaskManagement.Models
{
    public class BoardViewModel
    {
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public List<Board> Boards { get; set; } = new List<Board>();
        public Board? Board { get; set; } // İlk board'u saklamak için eklendi
        public string CurrentUser { get; set; } = string.Empty;
        public TaskItem TaskItem { get; set; } = new TaskItem();

        // Görevleri durumlarına göre ayırmak için özellikler
        public List<TaskItem> TodoTasks => Tasks.Where(t => t.Status == "todo").ToList();
        public List<TaskItem> DoingTasks => Tasks.Where(t => t.Status == "doing").ToList();
        public List<TaskItem> DoneTasks => Tasks.Where(t => t.Status == "done").ToList();
        
        public int TotalTaskCount => Tasks.Count;
        public int TotalBoardCount => Boards.Count;
        public List<SelectListItem> BoardSelectList { get; set; } = new List<SelectListItem>();
        
        public ICollection<Board> SharedBoards { get; set; } = new List<Board>();
    }
}