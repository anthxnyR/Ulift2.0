using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ulift2._0.Models
{
    public class Vehicle
    {
        [BsonId]
        public ObjectId Id;
        public String Plate { get; set; }
        public String Email { get; set; }
        public String Color { get; set; }
        public String Model { get; set; }
        public int Seats { get; set; }
    }
}
