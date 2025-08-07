


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demoproject.Data;
using Demoproject.Hubs;
using Demoproject.Hubs.Demoproject.Hubs;
using Demoproject.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Demoproject.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly QTraklyDBContext _context;
        private readonly IHubContext<FeedbackHub> _hubContext;

        public FeedbackService(QTraklyDBContext context, IHubContext<FeedbackHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendFeedbackAsync(Feedback feedback)
        {
            feedback.SentAt = feedback.SentAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(feedback.SentAt, DateTimeKind.Utc)
                : feedback.SentAt.ToUniversalTime();

            feedback.IsRead = false;

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Send real-time notification to the user
            await _hubContext.Clients.Group(feedback.UserId).SendAsync("newFeedback", feedback);
        }

        public async Task SendBulkFeedbackAsync(List<Feedback> feedbacks)
        {
            if (feedbacks == null || feedbacks.Count == 0) return;

            foreach (var fb in feedbacks)
            {
                fb.SentAt = fb.SentAt.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(fb.SentAt, DateTimeKind.Utc)
                    : fb.SentAt.ToUniversalTime();

                fb.IsRead = false;
            }

            _context.Feedbacks.AddRange(feedbacks);
            await _context.SaveChangesAsync();

            // Send real-time bulk notifications
            await _hubContext.Clients.Groups(feedbacks.Select(f => f.UserId).Distinct().ToList())
                .SendAsync("bulkFeedback", feedbacks);
        }

        public async Task<List<Feedback>> GetUnreadFeedbacksForUserAsync(string userId)
        {
            return await _context.Feedbacks
                .Where(f => f.UserId == userId )
                .OrderByDescending(f => f.SentAt)
                .ToListAsync();
        }

        public async Task<bool> MarkFeedbackAsReadAsync(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return false;

            if (!feedback.IsRead)
            {
                feedback.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> MarkAllFeedbacksAsReadAsync(string userId)
        {
            var unreadFeedbacks = await _context.Feedbacks
                .Where(f => f.UserId == userId && !f.IsRead)
                .ToListAsync();

            if (!unreadFeedbacks.Any()) return false;

            unreadFeedbacks.ForEach(f => f.IsRead = true);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}