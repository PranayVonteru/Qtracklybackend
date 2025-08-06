//namespace Demoproject.Models
//{
//    public class Feedback
//    {
//    }
//}
//using System;

//namespace Demoproject.Models
//{
//    public class Feedback
//    {
//        public int Id { get; set; }
//        public string UserId { get; set; }      // Receiver
//        public string ManagerId { get; set; }   // Sender
//        public string Message { get; set; }
//        public DateTime SentAt { get; set; } = DateTime.UtcNow;
//    }
//}

public class Feedback
{
    public int Id { get; set; }
    public string UserId { get; set; }      // Receiver
    public string ManagerId { get; set; }   // Sender
    public string Message { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false; // ✅ New column
}