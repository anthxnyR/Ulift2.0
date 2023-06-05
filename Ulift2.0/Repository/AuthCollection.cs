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

namespace Ulift2._0.Repository
{
    public class AuthCollection : IAuthCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<User> Collection;

        public AuthCollection()
        {
            Collection = _repository.db.GetCollection<User>("Users");
        }

        public async Task Login(string email, string password)
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
        }
        
        public async Task Register ([FromForm] User request)
        {
            Console.WriteLine(request);
            string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            string hash = BCrypt.Net.BCrypt.HashPassword(request.Password, salt);
            // if (request.Photo == null)
            // {
            //     System.Diagnostics.Trace.WriteLine("No hay foto de perfil");
            // }

            // string fileName = SaveImage(request.Photo);
            // System.Diagnostics.Trace.WriteLine(fileName);


            var newUser = new User
            {
                Email = request.Email,
                Password = hash,
                Name = request.Name,
                LastName = request.LastName,
                PhotoURL = request.PhotoURL,
                Gender = request.Gender,
                Role = request.Role,
                EmergencyContact = request.EmergencyContact,
                PassengerRating = 0,
                DriverRating = 0,
                ConfirmedUser = false
            };

            
            var result = await Collection.Find(x => x.Email == newUser.Email).FirstOrDefaultAsync();
            if (result == null)
            {
                var httpclient = new HttpClient();
                httpclient.BaseAddress = new Uri("https://localhost:7007");
                var content = new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "multipart/form-data");
                var response = await httpclient.PostAsync("/api/User", content);
                string domainPattern = @"@(est.ucab.edu.ve|ucab.edu.ve)$";
                if (!Regex.IsMatch(newUser.Email, domainPattern, RegexOptions.IgnoreCase))
                {
                    throw new Exception("El correo electrónico no pertenece al dominio UCAB");
                }

                if (response.IsSuccessStatusCode)  // No está funcionando (revisar)
                {
                    SendConfirmationEmail(newUser.Email, newUser.Name);
                }
                else
                {
                    throw new Exception("El usuario no pudo ser registrado");
                }  
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
                try
                {
                    var email = JwtService.GetTokenData(token);
                    var filter = Builders<User>.Filter.Eq<string>("Email", email);
                    var update = Builders<User>.Update.Set("ConfirmedUser", true);
                    var updateResult = await Collection.UpdateOneAsync(filter, update);


                    if (updateResult.MatchedCount > 0)
                    {
                        Console.WriteLine("Usuario verificado");
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

            var url = "https://localhost:7007/api/Auth/" + "Verify?token=" + tokenMailVerification;
            Console.WriteLine(url);

            var mailMessage = new MailMessage
            {
                From = new MailAddress("fi.ulift@gmail.com", "U-Lift"),
                Subject = "Confirmación de correo",
                Body = $"Estimado {recipientName}, \n\nPor favor, confirma tu dirección de correo dando click al siguiente link: \n\n{url}",
                IsBodyHtml = false
            };
            mailMessage.To.Add(new MailAddress(recipientEmail));

            smtpClient.Send(mailMessage);
        }

        // public string SaveImage(IFormFile file)
        // {
        //     string extension = Path.GetExtension(file.FileName);
        //     System.Diagnostics.Trace.WriteLine(extension);
        //     string fileName = Guid.NewGuid().ToString() + extension;
        //     string fileRoute = Path.Combine("images/", fileName);
        //     using (var stream = new FileStream(fileRoute, FileMode.Create))
        //     {
        //         file.CopyToAsync(stream);
        //     }
        //     return fileName.ToString();
        // }
    }
}
