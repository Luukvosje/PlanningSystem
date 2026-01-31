using PlanningSystem.Interfaces.DAL;
using PlanningSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.DAL.Repositories
{
    internal class UserRepository : GenericRepository<User> , IUserRepository
    {
        public UserRepository(AppSettings settings) : base(settings)
        {

        }

        public override User Create(User entity)
        {
            using (var context = GetContext())
            {
                var set = context.Set<User>();
                set.Add(entity);
                context.SaveChanges();

                this.ReloadCachedUsers();

                return entity;
            }
        }

        public void Delete(Guid guid)
        {
            throw new NotImplementedException();
        }

        public User? GetSingle(Guid guid)
        {
            throw new NotImplementedException();
        }

        public User? GetUserByEmail(string email)
        {
            using (var context = GetContext())
            {
                return (from g in context.Users where (g.Email != null && g.Email.Trim().ToLower() == (email + "").ToLower().Trim())
                        select g).FirstOrDefault();
            }
        }
    }
}
