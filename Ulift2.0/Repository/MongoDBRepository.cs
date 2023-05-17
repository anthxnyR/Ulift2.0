using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ulift2._0.Repository
{
    public class MongoDBRepository
    {
        public MongoClient client;
        public IMongoDatabase db;
        public MongoDBRepository()
        {
            client = new MongoClient("mongodb://mongo:pJEPTT2uLbKRGuWMGMKT@containers-us-west-179.railway.app:6721");
            db = client.GetDatabase("Ulift");
        }
    }
}
