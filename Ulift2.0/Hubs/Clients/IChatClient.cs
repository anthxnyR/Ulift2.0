using System.Threading.Tasks;
using Ulift2._0.Models;
using Ulift2._0.Hubs;

namespace Ulift2._0.Hubs.Clients
{
    public interface IChatClient
    {
        Task ReceiveMessage(string senderEmail ,string message);
    }
}
