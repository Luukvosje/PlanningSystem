using PlanningSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.Interfaces.DAL
{
    public interface IAppSettings
    {
        string? ConnectionString { get; set; }
        string? ApiTokenSecret { get; set; }
        string? APIUrl { get; set; }
        string? WebUrl { get; set; }
        string? Environment { get; set; }
    }
}
