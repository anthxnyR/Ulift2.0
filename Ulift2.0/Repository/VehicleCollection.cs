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
    public class VehicleCollection : IVehicleCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Vehicle> Collection;
        public VehicleCollection()
        {
            Collection = _repository.db.GetCollection<Vehicle>("Vehicles");
        }
        public async Task InsertVehicle(Vehicle vehicle)
        {
            await Collection.InsertOneAsync(vehicle);
        }
        public async Task UpdateVehicle(Vehicle vehicle)
        {
            var filter = Builders<Vehicle>.Filter.Eq(s => s.Id, vehicle.Id);
            await Collection.ReplaceOneAsync(filter, vehicle);
        }
        public async Task DeleteVehicle(String id)
        {
            var filter = Builders<Vehicle>.Filter.Eq(s => s.Id, new ObjectId(id));
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<Vehicle>> GetAllVehicles()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
        public void ValidateVehicleAttributes(Vehicle vehicle, ModelStateDictionary ModelState)
        {
            if (vehicle.Email == null)
            {
                ModelState.AddModelError("Email", "El vehículo debe tener un email asociado");
            }
            if (vehicle.Plate == null)
            {
                ModelState.AddModelError("LicensePlate", "El vehiculo debe tener una placa asociada");
            }
            if (vehicle.Model == null)
            {
                ModelState.AddModelError("Model", "El vehiculo debe tener un modelo asociado");
            }
            if (vehicle.Color == null)
            {
                ModelState.AddModelError("Color", "El vehiculo debe tener un color asociado");
            }
            if (vehicle.Seats == 0)
            {
                ModelState.AddModelError("Seats", "El vehiculo debe tener una cantidad de asientos asociada");
            }
        }
    }
}
