using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlanningSystem.Models.Models
{
 
    public class User : IGuid
    {
        [Key]
        public int Id { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }


        [JsonIgnore]
        public string? Pwd { get; set; }

        //public ICollection<OrganizationUserMap> OrganizationUserMaps { get; set; } = new List<OrganizationUserMap>();

        //public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }


}
