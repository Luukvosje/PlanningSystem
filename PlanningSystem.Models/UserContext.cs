using PlanningSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.Models
{
    public class UserContext
    {
        public required User User { get; set; }
        public required Organization Organisation { get; set; }
        public string? AccessToken { get; set; }
        public string? Context { get; set; }
        public string? Language { get; set; }
        public bool? IsPosing { get; set; }
        public Guid? PosedFromUserGuid { get; set; }
        public bool? SystemAdmin { get; set; }
        public bool? IsSystemRequest { get; set; } = false;
        public int OrganisationID { get { return Organisation?.Id ?? -1; } }

        // Additional property to access admin functions/modules in posing mode
        //public bool HasAdminAccess
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}

        public bool IsLoggedIn
        {
            get
            {
                return this.User != null;
            }
        }
    }

    public interface IGuid
    {
        Guid Guid { get; set; }
    }
}
