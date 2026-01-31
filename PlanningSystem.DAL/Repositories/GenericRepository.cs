using Microsoft.EntityFrameworkCore;
using PlanningSystem.Interfaces.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.DAL.Repositories
{
    internal class GenericRepository<T> : BaseRepository, IGenericRepository<T> where T : class
    {
        public GenericRepository(AppSettings settings) : base(settings)
        {

        }

        // Create
        public virtual T Create(T entity)
        {
            using (var context = GetContext())
            {
                var set = context.Set<T>();
                set.Add(entity);
                context.SaveChanges();

                return entity;
            }
        }

        // Read all
        public virtual List<T> GetAll()
        {
            using (var context = GetContext())
            {
                var set = context.Set<T>();

                return set.ToList();
            }
        }

        // Read
        public virtual T? GetSingle(int id)
        {
            using (var context = GetContext())
            {
                var set = context.Set<T>();

                return set.Find(id);
            }
        }

        // Read
        public virtual T? GetSingle(long id)
        {
            using (var context = GetContext())
            {
                var set = context.Set<T>();

                return set.Find(id);
            }
        }

        // Update
        public virtual void Save(T entity)
        {
            using (var context = GetContext())
            {
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        // Delete
        public void Delete(int id)
        {
            using (var context = GetContext())
            {
                var set = context.Set<T>();
                var entity = set.Find(id);
                if (entity != null)
                {
                    set.Remove(entity);
                    context.SaveChanges();
                }
            }
        }

        // Delete
        public void Delete(long id)
        {
            using (var context = GetContext())
            {
                var set = context.Set<T>();
                var entity = set.Find(id);
                if (entity != null)
                {
                    set.Remove(entity);
                    context.SaveChanges();
                }
            }
        }

        public virtual List<T> Find(Expression<Func<T, bool>> filter)
        {
            using (var context = GetContext())
            {
                var set = context.Set<T>();
                return set.Where(filter).ToList();
            }
        }
    }
}
