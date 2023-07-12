using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IMessageCollection
    {
        Task InsertMessage(Message message);
        Task UpdateMessage(Message message);
        Task DeleteMessage(String id);
        Task<IEnumerable<Message>> GetAllMessages();
        Task<List<Message>> GetAllMessagesFromUser(String senderEmail, String receiverEmail);
        void ValidateMessageAttributes(Message message, ModelStateDictionary ModelState);
    }
}
