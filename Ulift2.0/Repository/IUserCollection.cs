using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IUserCollection
    {
        Task InsertUser(User user);
        Task UpdateUser (User user);
        Task DeleteUser (String id);
        Task<IEnumerable<User>> GetAllUsers();
        void ValidateUserAttributes(User user, ModelStateDictionary ModelState);
        Task<object> GetUserInformation(string email);
    }
}
