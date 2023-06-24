using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Ulift2._0.Models
{
    public class Register
    {
        [BsonId]
        public ObjectId Id;
        public String Email { get; set; }
        public String Password { get; set; }
        public String Name { get; set; }
        public String LastName { get; set; }
        public IFormFile Photo { get; set; }
        public String Gender { get; set; }
        public String Role { get; set; }
        public String EmergencyContact { get; set; }
    }
}
