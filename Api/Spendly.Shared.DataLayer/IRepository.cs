namespace Spendly.Shared.DataLayer;

using Entities.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

public interface IRepository<T> where T : BaseEntity
{
	public string CollectionName { get; set; }
	Task<T?> GetAsync(Expression<Func<T, bool>> filter);
	Task<T?> GetAsync(string id);
	Task<T?> GetAsync(ObjectId id);
	Task<T?> GetAsync(Expression<Func<T, bool>> filter, ProjectionDefinition<T> projectionDefinition);
	Task<T?> GetAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition);
	Task<T?> GetAsync(ObjectId id, ProjectionDefinition<T> projectionDefinition);
	Task<T?> GetAsync(string id, ProjectionDefinition<T> projectionDefinition);
	Task<T> GetRequiredAsync(string id);
	Task<T> GetRequiredAsync(ObjectId id);
	Task<T> GetRequiredAsync(FilterDefinition<T> filterDefinition);
	Task<T> GetRequiredAsync(Expression<Func<T, bool>> filter);
	Task InsertAsync(T model);
	Task InsertManyAsync(List<T> models);
	Task UpdateAsync(T model);
	Task<UpdateResult> UpdateWithIdAsync(ObjectId id, UpdateDefinition<T> updateDef);
	Task<UpdateResult> UpdateAsync(FilterDefinition<T> filterDefinition, UpdateDefinition<T> updateDefinition);
	Task DeleteAsync(string id);
	Task DeleteAsync(ObjectId id);
	//Task<List<T>> ListAsync(FilterDefinition<T> filterDefinition);
	Task<List<T>> ListAsync(Expression<Func<T, bool>> filter, SortDefinition<T> sortDefinition, int limit);
	Task<List<T>> ListPagingAsync(Expression<Func<T, bool>>? filter = null, PagingParameter? paging = null, SortDefinition<T>? sortDefinition = null);
	Task<List<T>> ListAsync(FilterDefinition<T> filter, ProjectionDefinition<T>? projection = null);
	Task<List<T>> ListAsync(Expression<Func<T, bool>>? filter, ProjectionDefinition<T>? projection = null);
	Task<List<T>> ListAsync(IEnumerable<ObjectId> ids);
	Task<Dictionary<ObjectId, T>> ListDictionaryAsync(IEnumerable<ObjectId> ids);
	Task<Dictionary<ObjectId, T>> ListSingleDictionaryAsync(IEnumerable<ObjectId> ids,
		Expression<Func<T, ObjectId?>> propertySelector);
	Task<List<T>> ListPagingAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition, PagingParameter paging);
	Task<long> CountAsync(FilterDefinition<T> filterDefinition);
	Task<Dictionary<ObjectId, List<T>>> ListDictionaryAsync(IEnumerable<ObjectId> ids,
		Expression<Func<T, ObjectId?>> propertySelector);

}
