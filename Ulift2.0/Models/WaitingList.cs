using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ulift2._0.Models
{
    public class WaitingList
    {
        [BsonId]
        public ObjectId Id;
        public String DriverEmail { get; set; }
        public String PassengerEmail { get; set; }
    }
}
