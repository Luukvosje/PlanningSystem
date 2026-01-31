using PlanningSystem.Interfaces.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.Interfaces.DAL
{
    public interface IGenericRepository<T> where T : class
    {
        List<T> GetAll();
        T? GetSingle(int id);
        T? GetSingle(long id);
        T Create(T entity);
        void Save(T entity);
        void Delete(int id);
        void Delete(long id);

        List<T> Find(Expression<Func<T, bool>> filter);
    }
    public interface IGenericGuidRepository<T> : IGenericRepository<T> where T : class, IGuid
    {
        T? GetSingle(Guid guid);
        List<T> Find(Expression<Func<T, bool>> filter);

        void Delete(Guid guid);

    }

    public interface IGuid
    {
        Guid Guid { get; set; }
    }
}
