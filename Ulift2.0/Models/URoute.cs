using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ulift2._0.Models
{
    public class URoute
    {
        [BsonId]
        public ObjectId Id;
        public String Email { get; set; }
        public String Path { get; set; }
        public String Name { get; set; }
        public bool inUcab { get; set; }
    }
}
