namespace Spendly.Shared.DataLayer;

using MongoDB.Bson;
using MongoDB.Driver;
using Spendly.Shared.Entities.Core;
using System.Linq.Expressions;
using ViewModels.Settings;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly IMongoCollection<T?> _entities;
    private MongoClient Client { get; set; }

    private readonly FindOptions<T> _options;

    public string? CollectionName { get; set; }

    public Repository(DbSettings dbSettings)
    {
        var settings = new MongoClientSettings
        {
            Server = new MongoServerAddress("localhost", 27017),
            Credential = MongoCredential.CreateCredential(
                dbSettings.DatabaseName,      // authSource
                dbSettings.UserName,  // username
                dbSettings.Password             // password
            )
        };

        Client = new MongoClient(settings);
        var database = Client.GetDatabase(dbSettings.DatabaseName);

        var collation = new Collation("en", strength: CollationStrength.Secondary);
        _options = new FindOptions<T>
        {
            Collation = collation
        };

        _entities = database.GetCollection<T>(typeof(T).Name)!;
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> filter)
    {
        var entities = await _entities.FindAsync<T>(filter, _options).ConfigureAwait(false);
        return await entities.SingleOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(string id)
    {
        return await (await _entities.FindAsync(p => p.Id == ObjectId.Parse(id))).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(ObjectId id)
    {
        return await (await _entities.FindAsync(p => p.Id == id)).FirstOrDefaultAsync().ConfigureAwait(false);
    }
    public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, ProjectionDefinition<T> projectionDefinition)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };
        IAsyncCursor<T?> entities = await _entities.FindAsync<T>(filter, options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };
        var entities = await _entities.FindAsync<T>(filterDefinition, options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(ObjectId id, ProjectionDefinition<T> projectionDefinition)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };
        var entities = await _entities.FindAsync<T>(p => p.Id == id, options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(string id, ProjectionDefinition<T> projectionDefinition)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };
        var entities = await _entities.FindAsync<T>(p => p.Id == ObjectId.Parse(id), options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }
    
    public async Task<T> GetRequiredAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            throw new ArgumentException($"Invalid ObjectId: {id}");

        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
        };

        var entities = await _entities
            .FindAsync(p => p.Id == objectId, options)
            .ConfigureAwait(false);

        var entity = await entities.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
        {
            throw new Exception($"Id: {id}");
        }

        return entity;
    }
    
    public async Task<T> GetRequiredAsync(ObjectId id)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
        };

        var entities = await _entities
            .FindAsync(p => p.Id == id, options)
            .ConfigureAwait(false);

        var entity = await entities.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
        {
            throw new Exception($"Id: {id}");
        }

        return entity;
    }
    
    public async Task<T> GetRequiredAsync(FilterDefinition<T> filterDefinition)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation
        };

        var cursor = await _entities
            .FindAsync(filterDefinition, options)
            .ConfigureAwait(false);

        var entity = await cursor.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
        {
            throw new Exception("Entity not found for given filter.");
        }

        return entity;
    }
    
    public async Task<T> GetRequiredAsync(Expression<Func<T, bool>> filter)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation
        };

        var cursor = await _entities
            .FindAsync(filter, options)
            .ConfigureAwait(false);

        var entity = await cursor.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
        {
            throw new Exception("Entity not found for given expression filter.");
        }

        return entity;
    }




    public Task InsertAsync(T model)
    {
        model.CreatedAt = DateTime.UtcNow;
        return _entities.InsertOneAsync(model);
    }

    public Task UpdateAsync(T model)
    {
        model.UpdatedAt = DateTime.UtcNow;
        return _entities.ReplaceOneAsync(p => p.Id == model.Id, model);
    }
    public Task<UpdateResult> UpdateWithIdAsync(ObjectId id, UpdateDefinition<T> updateDef)
    {
        return _entities!.UpdateOneAsync(p => p.Id == id, updateDef);
    }

    public Task<UpdateResult> UpdateAsync(FilterDefinition<T> filterDefinition, UpdateDefinition<T> updateDefinition)
    {
        return _entities!.UpdateOneAsync(filterDefinition, updateDefinition);
    }

    public Task DeleteAsync(string id)
    {
        return _entities.DeleteOneAsync(p => p.Id == ObjectId.Parse(id));
    }

    public Task DeleteAsync(ObjectId id)
    {
        return _entities.DeleteOneAsync(p => p.Id == id);
    }

    public async Task<List<T>> ListAsync(FilterDefinition<T> filterDefinition)
    {
        var entities = await _entities.FindAsync<T>(filterDefinition).ConfigureAwait(false);
        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>> filter, SortDefinition<T> sortDefinition, int limit)
    {
        var entities = await _entities.Find<T>(filter)
            .Limit(limit)
            .Sort(sortDefinition)
            .ToCursorAsync();
        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ListPagingAsync(Expression<Func<T, bool>>? filter = null, PagingParameter? paging = null, SortDefinition<T>? sortDefinition = null)
    {
        filter ??= p => true;

        if (paging != null)
        {
            var findOptions = new FindOptions<T>
            {
                Skip = paging.PageNo * paging.PageSize,
                Limit = paging.PageSize
            };

            if (sortDefinition != null)
            {
                findOptions.Sort = sortDefinition;
            }

            var entities = await _entities.FindAsync(filter!, findOptions!).ConfigureAwait(false);
            return await entities.ToListAsync().ConfigureAwait(false);
        }
        else
        {
            var entities = await _entities.FindAsync<T>(filter!, _options!).ConfigureAwait(false);
            return await entities.ToListAsync().ConfigureAwait(false);
        }
    }

    public async Task<List<T>> ListAsync(FilterDefinition<T> filter, ProjectionDefinition<T>? projection = null)
    {
        var findOptions = new FindOptions<T>
        {
            Collation = _options.Collation
        };

        if (projection != null)
        {
            findOptions.Projection = projection;
        }

        var cursor = await _entities.FindAsync(filter, findOptions).ConfigureAwait(false);
        return await cursor.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? filter, ProjectionDefinition<T>? projection = null)
    {

        filter ??= p => true;
        var entities = await _entities.FindAsync<T>(filter, _options!).ConfigureAwait(false);
        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public async Task<Dictionary<ObjectId, T>> ListAsync(IEnumerable<ObjectId> ids)
    {
        var filter = Builders<T>.Filter.In(p => p.Id, ids.Distinct());
        var list = await _entities.Find(filter).ToListAsync().ConfigureAwait(false);
        return list.ToDictionary(e => e.Id);
    }
    
    public async Task<Dictionary<ObjectId, T>> ListAsync(
        IEnumerable<ObjectId> ids,
        Expression<Func<T, ObjectId?>> propertySelector)
    {
        var distinctIds = ids.Distinct().ToList();
        var nullableIds = distinctIds.Cast<ObjectId?>().ToList();

        // Null olmayan ve eşleşen kayıtlar
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.In(propertySelector, nullableIds),
            Builders<T>.Filter.Ne(propertySelector, default(ObjectId?))
        );

        var list = await _entities.Find(filter).ToListAsync().ConfigureAwait(false);

        var property = (propertySelector.Body as MemberExpression)?.Member;
        if (property == null)
            throw new ArgumentException("Property selector must be a property access expression.", nameof(propertySelector));

        var dictionary = list.ToDictionary(
            e => (ObjectId)typeof(T).GetProperty(property.Name)!.GetValue(e)!,
            e => e
        );

        return dictionary;
    }
    
    public async Task<Dictionary<ObjectId, List<T>>> ListAsync(
        IEnumerable<ObjectId> ids,
        Expression<Func<T, ObjectId?>> propertySelector,
        Func<T, ObjectId> keySelector)
    {
        var distinctIds = ids.Distinct().ToList();
        var nullableIds = distinctIds.Cast<ObjectId?>().ToList();

        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.In(propertySelector, nullableIds),
            Builders<T>.Filter.Ne(propertySelector, null)
        );

        var list = await _entities.Find(filter).ToListAsync();

        return list
            .GroupBy(keySelector)
            .ToDictionary(g => g.Key, g => g.ToList());
    }


    public async Task<List<T>> ListPagingAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition, PagingParameter paging)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition,
            Limit = paging.PageSize,
            Skip = paging.PageNo * paging.PageSize,
        };
        var entities = await _entities.FindAsync<T>(filterDefinition, options).ConfigureAwait(false);
        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public Task<long> CountAsync(FilterDefinition<T> filterDefinition)
    {
        var options = new CountOptions
        {
            Collation = _options.Collation,
        };
        return _entities.CountDocumentsAsync(filterDefinition, options);
    }

}
