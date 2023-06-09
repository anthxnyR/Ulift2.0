﻿using MongoDB.Bson;
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
    public class WaitingListCollection : IWaitingListCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<WaitingList> Collection;
        public WaitingListCollection()
        {
            Collection = _repository.db.GetCollection<WaitingList>("WaitingList");
        }

        public async Task InsertRequest(WaitingList waitingList)
        {
            var lift = await _repository.db.GetCollection<Lift>("Lifts").FindAsync(x => x.LiftId == waitingList.LiftId && x.Status == "A").Result.FirstOrDefaultAsync();
            if (lift == null)
            {
                throw new Exception("No existe un viaje activo.");
            }

            var user = await _repository.db.GetCollection<User>("Users").FindAsync(x => x.Email == waitingList.PassengerEmail).Result.FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("No existe el usuario.");
            }

            var wait = await _repository.db.GetCollection<WaitingList>("WaitingList").FindAsync(x => x.PassengerEmail == waitingList.PassengerEmail && x.LiftId == waitingList.LiftId).Result.FirstOrDefaultAsync();
            if (wait != null)
            {
                throw new Exception("Ya existe una solicitud de viaje.");
            }
            await Collection.InsertOneAsync(waitingList);
        }

        public async Task UpdateRequest(WaitingList list)
        {
            var filter = Builders<WaitingList>.Filter.Eq(s => s.LiftId, list.LiftId);
            await Collection.ReplaceOneAsync(filter, list);
        }
        public async Task DeleteRequest(String Id, String email)
        {
            var filter = Builders<WaitingList>.Filter.Eq(s => s.LiftId, Id) & Builders<WaitingList>.Filter.Eq(s => s.PassengerEmail, email);
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<WaitingList>> GetAllRequests()
        {
            
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
        public async Task<IEnumerable<WaitingListWithUser>> GetAllRequestsByLift(String LiftId)
        {
            var filter = Builders<WaitingList>.Filter.Eq(s => s.LiftId, LiftId);
            var waitingLists = await Collection.FindAsync(filter).Result.ToListAsync();

            var waitingListsWithUser = new List<WaitingListWithUser>();
            foreach (var waitingList in waitingLists)
            {
                var user = await _repository.db.GetCollection<User>("Users").FindAsync(x => x.Email == waitingList.PassengerEmail).Result.FirstOrDefaultAsync();
               
                var waitingListWithUser = new WaitingListWithUser
                {
                    WaitingList = waitingList,
                    User = user
                };

                waitingListsWithUser.Add(waitingListWithUser);
            }
            return waitingListsWithUser;
        }
    }
}
