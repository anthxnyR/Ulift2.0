using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;

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
        public async Task<IEnumerable<Message>> GetAllMessages()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
    }
}
