using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class BoardShare
    {
        public int Id { get; set; }

        [ForeignKey("Board")]
        public int BoardId { get; set; }
        public Board? Board { get; set; }



        [ForeignKey("SharedWithUser")]
        public string? SharedWithUserId { get; set; }
        public string?  SharedWithUserEmail { get; set; }
        public ApplicationUser? SharedWithUser { get; set; }
    }
}