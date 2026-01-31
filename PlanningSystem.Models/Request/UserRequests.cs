using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanningSystem.Models.Models;

namespace PlanningSystem.Models.Request
{
    [Serializable]
    public class UserAddRequest
    {
        public Guid Guid {  get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public required string Name { get; set; }
    }

    [Serializable]
    public class UserAuthRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    [Serializable]
    public class UserAuthResponse
    {
        public string Token { get; set; }
        public User User { get; set; }
    }
}
