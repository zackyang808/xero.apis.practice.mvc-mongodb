using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using xero.apis.practice.common.Contracts;

namespace xero.apis.practice.common.MongoDB
{
    public static class Extension
    {
        public static void AddMongoDB(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<MongoClient>(c =>
            {
                return new MongoClient(configuration["MongoDBOptions:connectionString"]);
            });

            services.TryAddScoped<IMongoDatabase>(c =>
            {
                var client = c.GetService<MongoClient>();
                return client.GetDatabase(configuration["MongoDBOptions:database"]);
            });

            services.TryAddTransient(typeof(IRepository<>), typeof(Repository<>));
        }
    }
}
