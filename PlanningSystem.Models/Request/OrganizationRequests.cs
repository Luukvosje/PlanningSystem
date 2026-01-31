using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.Models.Request
{
    [Serializable]
    public class OrganizationCreateRequest
    {
        public required string Name { get; set; }
    }

    [Serializable]
    public class OrganizationUpdateRequest
    {
        public required string Name { get; set; }
    }

    [Serializable]
    public class OrganizationAddUserRequest
    {
        public int UserId { get; set; }
        public string Role { get; set; } = "Member"; // Admin, Manager, Member, Viewer
    }
}
