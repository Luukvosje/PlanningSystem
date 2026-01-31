using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanningSystem.Interfaces.DAL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;

namespace PlanningSystem.Interfaces.BLL
{
    public interface IUserLogic
    {
        ResultObject<User> CreateUser(UserAddRequest request);
        ResultObject<UserAuthResponse> AuthenticateUser(UserAuthRequest request, string jwtKey);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateJwtToken(User user, string jwtKey);
        User GetUserByUsername(string email);
    }
}
