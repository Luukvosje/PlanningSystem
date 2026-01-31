using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.Models.Models
{
    public class Shift
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string Status { get; set; } = "scheduled"; 

        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public int WorkerId { get; set; }
        public User Worker { get; set; }
    }

    public enum ShiftStatusEnum
    {
        Scheduled = 0,
        Finished = 1,
        Missed = 2
    }

}
