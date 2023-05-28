using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.IO;
using Newtonsoft.Json;

namespace Ulift2._0.Helpers
{
    public class JwtService
    {
        private static byte[] salt = GenerateSalt();
        public static string GetToken(object payload)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("CLAVESECRETA");
            key = GenerateSecretKey(key, salt);
            var signingKey = new SymmetricSecurityKey(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("data", Newtonsoft.Json.JsonConvert.SerializeObject(payload))
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static dynamic? GetTokenData(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("CLAVESECRETA");

            key = GenerateSecretKey(key, salt);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                }, out SecurityToken validatedToken);
            
                var jwtToken = (JwtSecurityToken)validatedToken;
                var data = jwtToken.Claims.First(x => x.Type == "data").Value;
                return Newtonsoft.Json.JsonConvert.DeserializeObject(data);
            }catch(Exception e)
            {
                Console.WriteLine("Error al obtener el token: " + e.Message);
                return null;
            }

        }
        private static byte[] GenerateSecretKey(byte[] key, byte[] salt)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(key, salt, iterations: 10000);
            return deriveBytes.GetBytes(256 / 8);
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
