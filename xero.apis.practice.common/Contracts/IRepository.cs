using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace xero.apis.practice.common.Contracts
{
    public interface IRepository<T>
        where T : IEntity
    {
        Task<IEnumerable<T>> Get(Expression<Func<T, bool>> predicate, int skip, int take);
        Task<T> GetById(string id);
        Task Add(T entity);
        Task BulkAdd(List<T> list);
        Task Update(T entity);
        Task Delete(T entity);
    }
}
