using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Ulift2._0.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string userEmail, string message)
        {
            await Clients.User(userEmail).SendAsync("ReceiveMessage", message);
        }
    }
}
