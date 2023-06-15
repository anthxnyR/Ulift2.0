using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using System.Net.Mail;

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
            Console.WriteLine("Validating user attributes");
            if (user.Email == null)
            {
                ModelState.AddModelError("Email", "El usuario debe tener un email asociado");
            }
            else
            {
                string domainPattern = @"@(est.ucab.edu.ve|ucab.edu.ve)$";
                if (!Regex.IsMatch(user.Email, domainPattern, RegexOptions.IgnoreCase))
                {
                    Console.WriteLine("No es un correo UCAB");
                    ModelState.AddModelError("Email", "El correo electrónico no pertenece al dominio UCAB");
                    throw new Exception("El correo electrónico no pertenece al dominio UCAB");
                }
            }
            if (user.Password == null)
            {
                ModelState.AddModelError("Password", "El usuario debe tener una contraseña asociada");
            }
        }

        public async Task<object> GetUserInformation(string email)
        {
            var user = await Collection.Find(u => u.Email == email).FirstOrDefaultAsync();
            var vehicles = await _repository.db.GetCollection<Vehicle>("Vehicles").Find(v => v.Email == email).ToListAsync();
            var destinations = await _repository.db.GetCollection<Destination>("Destinations").Find(d => d.Email == email).ToListAsync();
            var routes = await _repository.db.GetCollection<URoute>("URoutes").Find(r => r.Email == email).ToListAsync();

            return new {
                User = user,
                Vehicles = vehicles,
                Destinations = destinations,
                URoutes = routes
            };
        }
    }
}
