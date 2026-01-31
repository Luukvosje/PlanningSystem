using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlanningSystem.Models.Models
{
    public class Organization
    {
        [Key]                             
        public int Id { get; set; } 
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<Shift> Shifts { get; set; }
        public ICollection<OrganizationUserMap> OrganizationUserMaps { get; set; }

    }

    public class OrganizationUserMap
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public OrganizationRole Role { get; set; } 
    }

    public enum OrganizationRole
    {
        Admin,
        Manager,
        Member,
        Viewer
    }


}


