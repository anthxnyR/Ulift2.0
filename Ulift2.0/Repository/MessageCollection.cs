using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ulift2._0.Repository
{
    public class MessageCollection : IMessageCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Message> Collection;
        public MessageCollection()
        {
            Collection = _repository.db.GetCollection<Message>("Messages");
        }
        public async Task InsertMessage(Message message)
        {
            await Collection.InsertOneAsync(message);
        }
        public async Task UpdateMessage(Message message)
        {
            var filter = Builders<Message>.Filter.Eq(s => s.Id, message.Id);
            await Collection.ReplaceOneAsync(filter, message);
        }
        public async Task DeleteMessage(String id)
        {
            var filter = Builders<Message>.Filter.Eq(s => s.Id, new ObjectId(id));
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<Message>> GetAllMessages()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        public async Task<List<Message>> GetAllMessagesFromUser(String senderEmail, String receiverEmail) { 
            var filter = Builders<Message>.Filter.Eq(s => s.SenderEmail, senderEmail);
            return await Collection.FindAsync(filter).Result.ToListAsync();
        }

        public void ValidateMessageAttributes(Message message, ModelStateDictionary ModelState)
        {
            if (message.Content == null)
            {
                ModelState.AddModelError("Context", "El mensaje debe tener un texto asociado");
            }
            if (message.SenderEmail == null)
            {
                ModelState.AddModelError("SenderEmail", "El mensaje debe tener un emisor asociado");
            }
            if (message.ReceiverEmail == null)
            {
                ModelState.AddModelError("ReceiverEmail", "El mensaje debe tener un receptor asociado");
            }
            if (message.LiftID == null)
            {
                ModelState.AddModelError("LiftID", "El mensaje debe tener un viaje asociado");
            }
        }
    }
}
