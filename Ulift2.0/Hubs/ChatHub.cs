using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Ulift2._0.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderEmail, string receiverEmail, string message)
        {
            await Clients.User(receiverEmail).SendAsync("ReceiveMessage", senderEmail, message);
        }
    }
}
