using MongoDB.Driver;
using MongoDB.Driver.Linq;
using xero.apis.practice.common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace xero.apis.practice.common.MongoDB
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly IMongoDatabase _database;
        private IMongoCollection<T> _collection => _database.GetCollection<T>(typeof(T).Name);

        public Repository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> predicate, int skip, int take)
        {
            return await _collection.AsQueryable().Where(predicate).ToListAsync();
        }

        public async Task<T> GetById(string id)
        {
            return await _collection.Find(x => x.ObjectId == id).FirstOrDefaultAsync();
        }

        public async Task Add(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                await _collection.InsertOneAsync(entity);
            }
            catch (Exception dbEx)
            {
                throw dbEx;
            }
        }

        public async Task BulkAdd(List<T> list)
        {
            try
            {
                await _collection.InsertManyAsync(list);
            }
            catch (Exception dbEx)
            {
                throw dbEx;
            }
        }

        public async Task Update(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                await _collection.ReplaceOneAsync(x => x.ObjectId == entity.ObjectId, entity, new ReplaceOptions { IsUpsert = true });
            }
            catch (Exception dbEx)
            {
                throw dbEx;
            }
        }

        public async Task Delete(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }

                await _collection.DeleteOneAsync(x => x.ObjectId == entity.ObjectId);
            }
            catch (Exception dbEx)
            {
                throw dbEx;
            }

        }
    }
}
