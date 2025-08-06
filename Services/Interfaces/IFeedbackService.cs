////using Demoproject.Models;

////namespace Demoproject.Services.Interfaces
////{
////    public interface IFeedbackService
////    {
////        Task SendFeedbackAsync(Feedback feedback);
////        Task<List<Feedback>> GetFeedbacksForUserAsync(string userId);
////    }
////}

using Demoproject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFeedbackService
{
    Task SendFeedbackAsync(Feedback feedback);

    Task SendBulkFeedbackAsync(List<Feedback> feedbacks);  // New method

    Task<List<Feedback>> GetUnreadFeedbacksForUserAsync(string userId);

    Task<bool> MarkFeedbackAsReadAsync(int feedbackId);

    Task<bool> MarkAllFeedbacksAsReadAsync(string userId);
}
