using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Demoproject.Hubs
{
    // Hubs/FeedbackHub.cs
    namespace Demoproject.Hubs
    {
        public class FeedbackHub : Hub
        {
            public async Task JoinUser(string userId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }


        }
    }
}
