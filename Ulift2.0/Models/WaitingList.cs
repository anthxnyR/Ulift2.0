using MongoDB.Bson.Serialization.Attributes;
namespace Ulift2._0.Models
{
    public class WaitingList
    {
        [BsonId]
        public Lift Lift { get; set; }
        public User Passenger { get; set; }
    }
}
