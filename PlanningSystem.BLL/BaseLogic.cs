using PlanningSystem.Interfaces.DAL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.BLL
{
    public abstract class BaseLogic
    {
        public IBllFactory BllFactory { get; set; }
        protected IDalFactory DalFactory { get { return BllFactory.DalFactory; } }

        public BaseLogic(IBllFactory bllFactory)
        {
            this.BllFactory = bllFactory;
        }
        protected ResultObject<T> SafeExecute<T>(Func<T> func, UserContext? user)
        {
            try
            {
                // Validate request
                if (user?.Organisation == null && user?.IsSystemRequest != true)
                    throw new Exception("Missing organisation");

                if (user?.User == null && user?.IsSystemRequest != true)
                    throw new Exception("Missing user");

                return new ResultObject<T>()
                {
                    Success = true,
                    Data = func(),
                };
            }
            catch (Exception e)
            {
                // Always return a ResultObject with the exception filled
                return new ResultObject<T>()
                {
                    Message = e.Message ?? "An error occurred",
                    Exception = e, // Always set the exception property
                    Success = false,
                    Data = default(T)
                };
            }
        }
        
        protected ResultObject<T> SafeExecute<T>(Func<T> func)
        {
            try
            {
                return new ResultObject<T>()
                {
                    Success = true,
                    Data = func(),
                };
            }
            catch (Exception e)
            {
                // Always return a ResultObject with the exception filled
                return new ResultObject<T>()
                {
                    Message = e.Message ?? "An error occurred",
                    Exception = e, // Always set the exception property
                    Success = false,
                    Data = default(T)
                };
            }
        }

    }
}
