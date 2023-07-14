using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Ulift2._0.Hubs.Clients;
using Ulift2._0.Models;

namespace Ulift2._0.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderEmail, string receiverEmail, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", senderEmail, receiverEmail, message);
        }
    }
}
