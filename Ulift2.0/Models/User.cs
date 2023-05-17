using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ulift2._0.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id;
        public String Email { get; set; }
        public String Password { get; set; }
        public String Name { get; set; }
        public String LastName { get; set; }
        public String PhotoURL { get; set; }
        public String Gender { get; set; }
        public String Role { get; set; }
        public String EmergencyContact { get; set; }
        public double PassengerRating { get; set; }
        public double DriverRating { get; set; }
    }
}
