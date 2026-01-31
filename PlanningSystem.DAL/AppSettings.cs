using PlanningSystem.Interfaces.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.DAL
{
    public class AppSettings : IAppSettings
    {
        public AppSettings(string connection)
        {
            this.ConnectionString = connection;
        }

        public AppSettings(IAppSettings other)
        {
            ConnectionString = other.ConnectionString;
        }


        public string? ConnectionString { get; set; }
        public string? ApiTokenSecret { get; set; }
        public string? APIUrl { get; set; }
        public string? WebUrl { get; set; }
        public string? Environment { get; set; }
    }
}
