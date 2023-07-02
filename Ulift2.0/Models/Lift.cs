using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ulift2._0.Models
{
    public class Lift
    {
        [BsonId]
        public ObjectId Id;
        public String LiftId { get; set; }
        public String Email1 { get; set; }
        public float Rating1 { get; set; }
        public bool Check1 { get; set; }
        public String Email2 { get; set; }
        public float Rating2 { get; set; }
        public bool Check2 { get; set; }
        public String Email3 { get; set; }
        public float Rating3 { get; set; }
        public bool Check3 { get; set; }
        public String Email4 { get; set; }
        public float Rating4 { get; set; }
        public bool Check4 { get; set; }
        public String Email5 { get; set; }
        public float Rating5 { get; set; }
        public bool Check5 { get; set; }
        public String DriverEmail { get; set; }
        public float DriverRating { get; set; }
        public String Status { get; set; }
        public String Plate { get; set; }
        public String Route { get; set; }
        public int Seats { get; set; }
        public int WaitingTime { get; set; }
        public DateTime CreatedAt { get; set; }
        // public bool complete { get; set; }
    }   
}
