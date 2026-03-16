using Microsoft.AspNetCore.SignalR;

namespace MiNegocioCR.Api.API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task Typing(string conversationId)
        {
            await Clients.OthersInGroup(conversationId)
                .SendAsync("typing", conversationId);
        }
    }
}
