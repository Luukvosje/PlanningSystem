using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.Models.Models
{
    public class ResultObject<T>
    {
        public string Message { get; set; }
        public Exception? Exception { get; set; }
        public bool Success { get; set; }
        public T Data { get; set; }
    }
}
