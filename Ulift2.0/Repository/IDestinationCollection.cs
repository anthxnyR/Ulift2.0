using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IDestinationCollection
    {
        Task InsertDestination (Destination destination);
        Task UpdateDestination (Destination destination);
        Task DeleteDestination (String id);
        Task<IEnumerable<Destination>> GetAllDestinations();
        void ValidateDestinationAttributes(Destination destination, ModelStateDictionary ModelState);
    }
}
