namespace sikayetvar.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }
        public string? Type { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string? Url { get; set; } 

    }
}
