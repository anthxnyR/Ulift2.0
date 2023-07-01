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
        Task DeleteRequest(String Id, String email);
        Task<IEnumerable<WaitingList>> GetAllRequests();
        Task UpdateRequest(WaitingList list);
        Task <IEnumerable<WaitingListWithUser>> GetAllRequestsByLift(String LiftId);
    }
}
