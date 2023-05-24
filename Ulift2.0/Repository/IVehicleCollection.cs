using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IVehicleCollection
    {
        Task InsertVehicle(Vehicle vehicle);
        Task UpdateVehicle(Vehicle vehicle);
        Task DeleteVehicle(String id);
        Task<IEnumerable<Vehicle>> GetAllVehicles();
        void ValidateVehicleAttributes(Vehicle vehicle, ModelStateDictionary ModelState);
    }
}
