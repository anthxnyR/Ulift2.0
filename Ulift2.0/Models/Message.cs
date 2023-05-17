using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ulift2._0.Models
{
    public class Message
    {
        [BsonId]
        public ObjectId Id;
        public String SenderEmail { get; set; }
        public String ReceiverEmail { get; set; }
        public String Content { get; set; }
        public DateTime DateTime { get; set; }
        public String LiftID { get; set; }

    }
}
