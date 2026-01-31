using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using PlanningSystem.Interfaces.BLL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.BLL
{
    public class UserLogic : BaseLogic, IUserLogic
    {
        public UserLogic(BllFactory bllFactory) : base(bllFactory) { }

        public ResultObject<User> CreateUser(UserAddRequest request)
        {
            return SafeExecute(() =>
            {
                if (string.IsNullOrWhiteSpace(request?.Email))
                    throw new Exception("Missing 'Email' value");

                if (string.IsNullOrWhiteSpace(request?.Password))
                    throw new Exception("Missing 'Password' value");

                var user = BllFactory.UserLogic.GetUserByUsername((request.Email ?? "").Trim());
                if (user != null)
                    throw new Exception("User already exists");

                //var org = new Organization()
                //{
                //   Id = Guid.NewGuid(),
                //};

                //org = DalFactory.OrganisationRepository.Create(org);

                //// Create OpenAI key
                //VerifyOrganisationApiKeys(org);

                // Create new user
                var newUser = new User()
                {
                    Guid = Guid.NewGuid(),
                    //OrganisationID = org.OrganisationID,
                    Email = (request?.Email ?? "").Trim().ToLower(),
                    Name = (request.Name ?? "").Trim(),
                    //IsActive = true,
                    //IsManager = true,
                    //CreatedDateUtc = DateTime.UtcNow,
                    //IsAdministrator = false,
                };

                newUser.Pwd = this.HashPassword(request.Password ?? "test");

                // Save to DB
                newUser = DalFactory.UserRepository.Create(newUser);

                //return newUser;
                return newUser;
            });
        }

        public string HashPassword(string password)
        {
            var salt = Helpers.Base64Helper.From("YWFw");

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        public User GetUserByUsername(string email)
        {
            return DalFactory.UserRepository.GetUserByEmail(email);
        }

        public ResultObject<UserAuthResponse> AuthenticateUser(UserAuthRequest request, string jwtKey)
        {
            return SafeExecute(() =>
            {
                if (string.IsNullOrWhiteSpace(request?.Email))
                    throw new Exception("Email is required");

                if (string.IsNullOrWhiteSpace(request?.Password))
                    throw new Exception("Password is required");

                var user = GetUserByUsername(request.Email.Trim().ToLower());
                if (user == null)
                    throw new Exception("Invalid email or password");

                if (string.IsNullOrWhiteSpace(user.Pwd))
                    throw new Exception("Invalid email or password");

                if (!VerifyPassword(request.Password, user.Pwd))
                    throw new Exception("Invalid email or password");

                var token = GenerateJwtToken(user, jwtKey);

                return new UserAuthResponse
                {
                    Token = token,
                    User = new User
                    {
                        Id = user.Id,
                        Guid = user.Guid,
                        Name = user.Name,
                        Email = user.Email
                    }
                };
            });
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var salt = Helpers.Base64Helper.From("YWFw");
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password!,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                return hashed == hashedPassword;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateJwtToken(User user, string jwtKey)
        {
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Name, user.Name ?? ""),
                    new Claim("Guid", user.Guid.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
