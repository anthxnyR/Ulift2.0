using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ulift2._0.Models
{
    public class LiftCreation
    {
        [BsonId]
        public ObjectId Id;
        public String DriverEmail { get; set; }
        public String Plate { get; set; }
        public String Route { get; set; }
        public int Seats { get; set; }
        public int WaitingTime { get; set; }
    }
}
