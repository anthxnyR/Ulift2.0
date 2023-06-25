using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ulift2._0.Models
{
    public class AvailableLift
    {
        public Lift Lift { get; set; }
        public User Driver { get; set; }
        public URoute Route { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
