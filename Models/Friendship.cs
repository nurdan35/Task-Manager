namespace TaskManagement.Models
{
    public class Friendship
    {
        public int Id { get; set; } // Unique ID for the friendship relationship
        public string RequesterId { get; set; } = string.Empty; // User sending the request
        public string ReceiverId { get; set; } = string.Empty; // User receiving the request
        public bool IsAccepted { get; set; }    // Is the request accepted?
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp of request creation

        public ApplicationUser? Requester { get; set; } // Details of the requester
        public ApplicationUser? Receiver { get; set; }  // Details of the receiver
    }
}