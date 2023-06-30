using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IWaitingListCollection
    {
        Task InsertRequest(WaitingList list);
        Task DeleteRequest(string id);
        Task<IEnumerable<WaitingList>> GetAllRequests();
        Task UpdateRequest(WaitingList list);
        Task <IEnumerable<WaitingList>> GetAllRequestsByLift(String LiftId);
    }
}
