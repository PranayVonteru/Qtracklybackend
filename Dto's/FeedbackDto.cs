//namespace Demoproject.Dto_s
//{
//    public class FeedbackDto
//    {
//        public string UserId { get; set; } // ID of the user receiving feedback
//        public string Message { get; set; } // Feedback message
//    }
//}

namespace Demoproject.Dto_s
{
    public class FeedbackDto
    {
        public string UserId { get; set; } // ID of the user receiving feedback
        public string Message { get; set; } // Feedback message
    }

    public class BulkFeedbackDto
    {
        public List<string> UserIds { get; set; }  // List of user IDs to notify
        public string Message { get; set; }        // Feedback message
    }
}