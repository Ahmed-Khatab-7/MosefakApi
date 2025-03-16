namespace MosefakApp.Core.IRepositories.Generic
{
    public interface IGenericRepositoryAsync<T> where T : class
    {
        Task<IList<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task<(IList<T> items,int totalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<(IList<T> items, int totalCount)> GetAllAsync(Func<IQueryable<T>, IQueryable<T>> include = null, int pageNumber = 1, int pageSize = 10);
        Task<(IList<T> items, int totalCount)> GetAllAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>[] includes = null, int pageNumber = 1, int pageSize = 10);
        Task<(IList<T> items,int totalCount)> GetAllAsync(Expression<Func<T,bool>> expression, Func<IQueryable<T>, IQueryable<T>> include = null, int pageNumber = 1, int pageSize = 10);
        Task<(IList<T> items, int totalCount)> GetAllAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>> include = null, Expression<Func<T, object>> sortBy = null,bool isDes = false, int pageNumber = 1, int pageSize = 10);
        Task<T> GetByIdAsync(object id);
        Task<double> GetAverageAsync(Expression<Func<T,double>> expression, Expression<Func<T,bool>> criteria);
        Task<double> GetAverageAsync(Expression<Func<T,double>> expression);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> include = null);
        Task<long> GetCountAsync();
        Task<long> GetCountWithConditionAsync(Expression<Func<T, bool>> condition);
        Task<long> GetCountWithConditionAsync(Expression<Func<T, bool>> condition, Func<IQueryable<T>, IQueryable<T>> include = null);
        Task AddEntityAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateEntityAsync(T entity);
        Task ExecuteUpdateAsync(Expression<Func<T, bool>> filter, Expression<Func<T, T>> updateExpression);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        Task DeleteEntityAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
    }
}
