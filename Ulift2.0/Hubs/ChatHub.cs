using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Ulift2._0.Hubs.Clients;
using Ulift2._0.Models;

namespace Ulift2._0.Hubs
{
    public class ChatHub : Hub<IChatClient>
    {
        public async Task SendMessage(string userEmail, string message)
        {
            await Clients.User(userEmail).SendAsync("ReceiveMessage", message);
        }
    }
}
