using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ulift2._0.Models
{
    public class Destination
    {
        [BsonId]
        public ObjectId Id;
        public String Email { get; set; }
        public String Name { get; set; }
        public Double Lat { get; set; }
        public Double Lng { get; set; }

    }
}
