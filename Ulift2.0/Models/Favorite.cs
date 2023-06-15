using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ulift2._0.Models
{
    public class Favorite
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string UserEmail { get; set; }
        public string FavoriteEmail { get; set; }
    }
}
