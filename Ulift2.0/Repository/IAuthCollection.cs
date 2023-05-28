using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulift2._0.Models;
namespace Ulift2._0.Repository
{
    public interface IAuthCollection
    {
        //Task Login(User user);
        Task Register(User user);
        Task Verify(string Token);
        void SendConfirmationEmail(String recipientEmail, string recipientName);
    }
}
