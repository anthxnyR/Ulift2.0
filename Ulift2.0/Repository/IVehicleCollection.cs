using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IVehicleCollection
    {
        Task<IEnumerable<Vehicle>> GetAllVehicles();
    }
}
