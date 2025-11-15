using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace PollApp.Hubs
{
    public class ResultsHub : Hub
    {
        public async Task JoinPoll(int pollId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"poll_{pollId}");
        }
    }
}