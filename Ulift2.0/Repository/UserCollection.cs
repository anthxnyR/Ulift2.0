using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ulift2._0.Repository
{
    public class UserCollection : IUserCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<User> Collection;
        public UserCollection()
        {
            Collection = _repository.db.GetCollection<User>("Users");
        }
        public async Task InsertUser(User user)
        {
            await Collection.InsertOneAsync(user);
        }
        public async Task UpdateUser(User user)
        {
            var filter = Builders<User>.Filter.Eq(s => s.Id, user.Id);
            await Collection.ReplaceOneAsync(filter, user);
        }
        public async Task DeleteUser(String id)
        {
            var filter = Builders<User>.Filter.Eq(s => s.Id, new ObjectId(id));
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
        public void ValidateUserAttributes(User user, ModelStateDictionary ModelState)
        {
            if (user.Email == null)
            {
                ModelState.AddModelError("Email", "El usuario debe tener un email asociado");
            }
            if (user.Password == null)
            {
                ModelState.AddModelError("Password", "El usuario debe tener una contraseña asociada");
            }
        }
    }
}
