﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
using Ulift2._0.Helpers;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using Ulift2._0.Controllers;
using MongoDB.Driver.Core.Configuration;
using Azure.Storage.Blobs;

namespace Ulift2._0.Repository
{
    public class AuthCollection : IAuthCollection
    {

        private readonly ILogger<AuthCollection> _logger;

        public AuthCollection(ILogger<AuthCollection> logger)
        {
            _logger = logger;
        }

        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<User> Collection;

        public AuthCollection()
        {
            Collection = _repository.db.GetCollection<User>("Users");
        }

        public async Task<bool> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("El correo electrónico es obligatorio", nameof(email));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("La contraseña es obligatoria", nameof(password));
            }

            var user = await Collection.Find(x => x.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("El usuario no existe");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                throw new Exception("Contraseña incorrecta");
            }

            if (!user.ConfirmedUser)
            {
                throw new Exception("El usuario no está verificado");
            }

            Log.Information("Usuario logueado");
            return true;
        }
        
        public async Task Register ([FromForm] Register request)
        {
            Console.WriteLine(request);
            string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            string hash = BCrypt.Net.BCrypt.HashPassword(request.Password, salt);


            var newUser = new User
            {
                Email = request.Email,
                Password = hash,
                Name = request.Name,
                LastName = request.LastName,
                Gender = request.Gender,
                Role = request.Role,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContact = request.EmergencyContact,
                PassengerRating = 0,
                DriverRating = 0,
                ConfirmedUser = false,
                LiftCountAsDriver = 0,
                LiftCountAsPassenger = 0,
                Status = "P",
            };

            
            var result = await Collection.Find(x => x.Email == newUser.Email).FirstOrDefaultAsync();
            if (result == null)
            {
                var httpclient = new HttpClient();
                Console.WriteLine(newUser.PhotoURL);
                httpclient.BaseAddress = new Uri("https://u-lift.azurewebsites.net");
                string domainPattern = @"@(est.ucab.edu.ve|ucab.edu.ve)$";
                if (!Regex.IsMatch(newUser.Email, domainPattern, RegexOptions.IgnoreCase))
                {
                    throw new Exception("El correo electrónico no pertenece al dominio UCAB");
                }
                string blobFileName = newUser.Email + ".jpg";

                string blobConnectionString = "DefaultEndpointsProtocol=https;AccountName=uliftprofilepictures;AccountKey=SgdMTcwETsdjZWmHKjyX0oqxbxER7xRiY7dqYmnk77R1xzVNLdyv9LZLZw8KbwLvdcTP3yEkZ4ME+ASt9YE/Bg==;EndpointSuffix=core.windows.net";
                string blobContainerName = "profilepictures";
                BlobContainerClient containerClient = new BlobContainerClient(blobConnectionString, blobContainerName);

                using (Stream stream = request.Photo.OpenReadStream())
                {
                    await containerClient.UploadBlobAsync(blobFileName, stream);
                }

                newUser.PhotoURL = containerClient.Uri + "/" + blobFileName;
                var content = new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json");
                Console.WriteLine($"{newUser.PhotoURL}");
                var response = await httpclient.PostAsync("/api/User", content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("No se pudo registrar el usuario");
                }
                SendConfirmationEmail(newUser.Email, newUser.Name);
            }
            else
            {
                throw new Exception("El usuario ya existe");
            }
        }


        public async Task Verify(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                try {
                
                    var email = JwtService.GetTokenData(token);
                    var filter = Builders<User>.Filter.Eq<string>("Email", email);
                    var update = Builders<User>.Update.Set("ConfirmedUser", true);
                    
                    var updateResult = await Collection.UpdateOneAsync(filter, update);

                    if (updateResult.MatchedCount > 0)
                    {
                        Console.WriteLine("Usuario verificado");
                    }
                    else
                    {
                        throw new Exception("No se realizo el cambio");
                    }
                }catch(Exception e)
                {
                    throw new Exception("El token no es válido");
                }        
            }
            else
            {
                throw new Exception("El token no es válido");
            }
        }

        public void SendConfirmationEmail(string recipientEmail, string recipientName)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("fi.ulift@gmail.com", "aprgomokkorhwdkg"),
                EnableSsl = true
            };

            string tokenMailVerification = JwtService.GetToken(recipientEmail);

            //Cambiar variable url por la el dominio azure
            var url = "https://u-lift.azurewebsites.net/api/Auth/" + "Verify?token=" + tokenMailVerification;
            Console.WriteLine(url);

            string filePath = @"Assets\EmailConfirmation.html";
            string htmlBody = File.ReadAllText(filePath);

            var mailMessage = new MailMessage
            {
                From = new MailAddress("fi.ulift@gmail.com", "U-Lift"),
                Subject = "Confirmación de correo",
                Body = htmlBody.Replace("%url%", url).Replace("%USUARIO%", recipientName),
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(recipientEmail));

            smtpClient.Send(mailMessage);
        }

        //public string SaveImage(IFormFile file)
        //{
        //    string extension = Path.GetExtension(file.FileName);
        //    System.Diagnostics.Trace.WriteLine(extension);
        //    string filename = Guid.NewGuid().ToString() + extension;
        //    string fileroute = Path.Combine("images/", filename);
        //    try
        //    {
        //        using (var fileStream = new FileStream(fileroute, FileMode.Create))
        //        {
        //            file.CopyTo(fileStream);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        System.Diagnostics.Trace.WriteLine("nosexd");
        //    }
        //    return filename.ToString();
        //}
    }
}
