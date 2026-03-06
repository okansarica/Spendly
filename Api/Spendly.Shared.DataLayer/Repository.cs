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
        int port = 27017;
        var portEnv = Environment.GetEnvironmentVariable("TEST_MONGO_PORT");

        if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var parsed))
        {
            port = parsed;
        }

        var settings = new MongoClientSettings
        {
            Server = new MongoServerAddress(Environment.GetEnvironmentVariable("TEST_MONGO_HOST") ?? "localhost", port),
        };

        if (!string.IsNullOrEmpty(dbSettings?.UserName))
        {
            settings.Credential = MongoCredential.CreateCredential(
                dbSettings.DatabaseName,
                dbSettings.UserName,
                dbSettings.Password
            );
        }

        Client = new MongoClient(settings);
        var database = Client.GetDatabase(dbSettings.DatabaseName);

        var collation = new Collation("en", strength: CollationStrength.Secondary);

        _options = new FindOptions<T>
        {
            Collation = collation
        };

        _entities = database.GetCollection<T>(typeof(T).Name)!;
    }

    private static bool IsSoftDeleteEntity =>
        typeof(ISoftDeletable).IsAssignableFrom(typeof(T));

    private FilterDefinition<T> ApplySoftDeleteFilter(FilterDefinition<T> filter)
    {
        if (!IsSoftDeleteEntity)
            return filter;

        var softDeleteFilter = Builders<T>.Filter.Where(
            x => !((ISoftDeletable)x).IsDeleted
        );

        return Builders<T>.Filter.And(filter, softDeleteFilter);
    }

    private FilterDefinition<T> ApplySoftDeleteFilter(Expression<Func<T, bool>> filter)
    {
        var baseFilter = Builders<T>.Filter.Where(filter);

        if (!IsSoftDeleteEntity)
            return baseFilter;
        
        var softDeleteFilter = Builders<T>.Filter.Where(
            x => !((ISoftDeletable)x).IsDeleted
        );

        return Builders<T>.Filter.And(baseFilter, softDeleteFilter);
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> filter)
    {
        var mongoFilter = ApplySoftDeleteFilter(filter);

        var entities = await _entities.FindAsync<T>(mongoFilter, _options).ConfigureAwait(false);
        return await entities.SingleOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
        filter = ApplySoftDeleteFilter(filter);

        return await (await _entities.FindAsync(filter)).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(ObjectId id)
    {
        var filter = Builders<T>.Filter.Eq(p => p.Id, id);
        filter = ApplySoftDeleteFilter(filter);

        return await (await _entities.FindAsync(filter)).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, ProjectionDefinition<T> projectionDefinition)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };

        var mongoFilter = ApplySoftDeleteFilter(filter);

        var entities = await _entities.FindAsync<T>(mongoFilter, options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition)
    {
        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };

        filterDefinition = ApplySoftDeleteFilter(filterDefinition);

        var entities = await _entities.FindAsync<T>(filterDefinition, options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(ObjectId id, ProjectionDefinition<T> projectionDefinition)
    {
        var filter = Builders<T>.Filter.Eq(p => p.Id, id);
        filter = ApplySoftDeleteFilter(filter);

        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };

        var entities = await _entities.FindAsync<T>(filter, options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetAsync(string id, ProjectionDefinition<T> projectionDefinition)
    {
        var filter = Builders<T>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
        filter = ApplySoftDeleteFilter(filter);

        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition
        };

        var entities = await _entities.FindAsync<T>(filter, options).ConfigureAwait(false);
        return await entities.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T> GetRequiredAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            throw new ArgumentException($"Invalid ObjectId: {id}");

        var filter = Builders<T>.Filter.Eq(p => p.Id, objectId);
        filter = ApplySoftDeleteFilter(filter);

        var entities = await _entities.FindAsync(filter, _options).ConfigureAwait(false);

        var entity = await entities.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
            throw new Exception($"Id: {id}");

        return entity;
    }

    public async Task<T> GetRequiredAsync(ObjectId id)
    {
        var filter = Builders<T>.Filter.Eq(p => p.Id, id);
        filter = ApplySoftDeleteFilter(filter);

        var entities = await _entities.FindAsync(filter, _options).ConfigureAwait(false);

        var entity = await entities.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
            throw new Exception($"Id: {id}");

        return entity;
    }

    public async Task<T> GetRequiredAsync(FilterDefinition<T> filterDefinition)
    {
        filterDefinition = ApplySoftDeleteFilter(filterDefinition);

        var cursor = await _entities.FindAsync(filterDefinition, _options).ConfigureAwait(false);

        var entity = await cursor.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
            throw new Exception("Entity not found for given filter.");

        return entity;
    }

    public async Task<T> GetRequiredAsync(Expression<Func<T, bool>> filter)
    {
        var mongoFilter = ApplySoftDeleteFilter(filter);

        var cursor = await _entities.FindAsync(mongoFilter, _options).ConfigureAwait(false);

        var entity = await cursor.FirstOrDefaultAsync().ConfigureAwait(false);

        if (entity is null)
            throw new Exception("Entity not found for given expression filter.");

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
        return _entities.UpdateOneAsync(p => p.Id == id, updateDef);
    }

    public Task<UpdateResult> UpdateAsync(FilterDefinition<T> filterDefinition, UpdateDefinition<T> updateDefinition)
    {
        return _entities.UpdateOneAsync(filterDefinition, updateDefinition);
    }

    public Task DeleteAsync(string id)
    {
        return DeleteAsync(ObjectId.Parse(id));
    }

    public Task DeleteAsync(ObjectId id)
    {
        if (IsSoftDeleteEntity)
        {
            var update = Builders<T>.Update
                .Set("IsDeleted", true)
                .Set("DeletedAt", DateTime.UtcNow);

            return _entities.UpdateOneAsync(p => p.Id == id, update);
        }

        return _entities.DeleteOneAsync(p => p.Id == id);
    }

    public async Task<List<T>> ListAsync(FilterDefinition<T> filterDefinition)
    {
        filterDefinition = ApplySoftDeleteFilter(filterDefinition);

        var entities = await _entities.FindAsync(filterDefinition).ConfigureAwait(false);
        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>> filter, SortDefinition<T> sortDefinition, int limit)
    {
        var mongoFilter = ApplySoftDeleteFilter(filter);

        var entities = await _entities.Find(mongoFilter)
            .Limit(limit)
            .Sort(sortDefinition)
            .ToCursorAsync();

        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ListPagingAsync(Expression<Func<T, bool>>? filter = null, PagingParameter? paging = null, SortDefinition<T>? sortDefinition = null)
    {
        filter ??= p => true;

        var mongoFilter = ApplySoftDeleteFilter(filter);

        if (paging != null)
        {
            var findOptions = new FindOptions<T>
            {
                Skip = paging.PageNo * paging.PageSize,
                Limit = paging.PageSize
            };

            if (sortDefinition != null)
                findOptions.Sort = sortDefinition;

            var entities = await _entities.FindAsync(mongoFilter, findOptions).ConfigureAwait(false);
            return await entities.ToListAsync().ConfigureAwait(false);
        }

        var cursor = await _entities.FindAsync(mongoFilter, _options).ConfigureAwait(false);
        return await cursor.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ListAsync(FilterDefinition<T> filter, ProjectionDefinition<T>? projection = null)
    {
        filter = ApplySoftDeleteFilter(filter);

        var findOptions = new FindOptions<T>
        {
            Collation = _options.Collation
        };

        if (projection != null)
            findOptions.Projection = projection;

        var cursor = await _entities.FindAsync(filter, findOptions).ConfigureAwait(false);
        return await cursor.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? filter, ProjectionDefinition<T>? projection = null)
    {
        filter ??= p => true;

        var mongoFilter = ApplySoftDeleteFilter(filter);

        var entities = await _entities.FindAsync(mongoFilter, _options).ConfigureAwait(false);
        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public async Task<Dictionary<ObjectId, T>> ListAsync(IEnumerable<ObjectId> ids)
    {
        var filter = Builders<T>.Filter.In(p => p.Id, ids.Distinct());
        filter = ApplySoftDeleteFilter(filter);

        var list = await _entities.Find(filter).ToListAsync().ConfigureAwait(false);
        return list.ToDictionary(e => e.Id);
    }

    public async Task<Dictionary<ObjectId, List<T>>> ListAsync(
        IEnumerable<ObjectId> ids,
        Expression<Func<T, ObjectId?>> propertySelector)
    {
        var distinctIds = ids.Distinct().ToList();
        var nullableIds = distinctIds.Cast<ObjectId?>().ToList();

        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.In(propertySelector, nullableIds),
            Builders<T>.Filter.Ne(propertySelector, default(ObjectId?))
        );

        filter = ApplySoftDeleteFilter(filter);

        var list = await _entities.Find(filter).ToListAsync().ConfigureAwait(false);

        var property = (propertySelector.Body as MemberExpression)?.Member;

        if (property == null)
            throw new ArgumentException("Property selector must be a property access expression.", nameof(propertySelector));

        var propInfo = typeof(T).GetProperty(property.Name)!;

        var dictionary = list
            .GroupBy(e => (ObjectId)propInfo.GetValue(e)!)
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
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

        filter = ApplySoftDeleteFilter(filter);

        var list = await _entities.Find(filter).ToListAsync();

        return list
            .GroupBy(keySelector)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<List<T>> ListPagingAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition, PagingParameter paging)
    {
        filterDefinition = ApplySoftDeleteFilter(filterDefinition);

        var options = new FindOptions<T>
        {
            Collation = _options.Collation,
            Projection = projectionDefinition,
            Limit = paging.PageSize,
            Skip = paging.PageNo * paging.PageSize,
        };

        var entities = await _entities.FindAsync(filterDefinition, options).ConfigureAwait(false);
        return await entities.ToListAsync().ConfigureAwait(false);
    }

    public Task<long> CountAsync(FilterDefinition<T> filterDefinition)
    {
        filterDefinition = ApplySoftDeleteFilter(filterDefinition);

        var options = new CountOptions
        {
            Collation = _options.Collation,
        };

        return _entities.CountDocumentsAsync(filterDefinition, options);
    }
}
