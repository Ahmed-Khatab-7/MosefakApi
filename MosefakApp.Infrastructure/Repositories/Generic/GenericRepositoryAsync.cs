using Microsoft.EntityFrameworkCore;

namespace MosefakApp.Infrastructure.Repositories.Generic
{
    public class GenericRepositoryAsync<T> : IGenericRepositoryAsync<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _entity;

        public GenericRepositoryAsync(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entity = _context.Set<T>();
        }

        public DbSet<T> Entity => _entity;

        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate == null ? await _entity.AnyAsync() : await _entity.AnyAsync(predicate);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            var query = include?.Invoke(_entity) ?? _entity;
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IList<T>> GetAllAsync(Expression<Func<T, bool>> expression)
            => await _entity.Where(expression).AsNoTracking().ToListAsync();

        public async Task<(IList<T> items, int totalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10)
            => await GetPagedDataAsync(_entity, pageNumber, pageSize);

        public async Task<(IList<T> items, int totalCount)> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null, int pageNumber = 1, int pageSize = 10)
            => await GetPagedDataAsync(include?.Invoke(_entity) ?? _entity, pageNumber, pageSize);

        public async Task<(IList<T> items, int totalCount)> GetAllAsync(
        Expression<Func<T, bool>> expression,
        Func<IQueryable<T>, IQueryable<T>>[]? includes = null,
        int pageNumber = 1,
        int pageSize = 10)
        {
            IQueryable<T> query = _entity.Where(expression); 

            if (includes is not null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            // Get total count before applying pagination
            int totalCount = await query.CountAsync();

            pageNumber = Math.Max(1, pageNumber); // Ensure positive page number

            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }


        public async Task<(IList<T> items, int totalCount)> GetAllAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>? include = null, int pageNumber = 1, int pageSize = 10)
            => await GetPagedDataAsync((include?.Invoke(_entity) ?? _entity).Where(expression), pageNumber, pageSize);

        public async Task<(IList<T> items, int totalCount)> GetAllAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>? include = null, Expression<Func<T, object>>? sortBy = null, bool isDescending = false, int pageNumber = 1, int pageSize = 10)
        {
            var query = (include?.Invoke(_entity) ?? _entity).Where(expression);
            if (sortBy is not null)
                query = isDescending ? query.OrderByDescending(sortBy) : query.OrderBy(sortBy);

            return await GetPagedDataAsync(query, pageNumber, pageSize);
        }

        public async Task<T?> GetByIdAsync(object id)
            => await _entity.FindAsync(id);

        public async Task<long> GetCountAsync()
            => await _entity.LongCountAsync();

        public async Task<long> GetCountWithConditionAsync(Expression<Func<T, bool>> condition)
            => await _entity.LongCountAsync(condition);

        public async Task<long> GetCountWithConditionAsync(Expression<Func<T, bool>> condition, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            var query = (include?.Invoke(_entity) ?? _entity).Where(condition);
            return await query.LongCountAsync();
        }

        public async Task<double> GetAverageAsync(Expression<Func<T, double>> expression, Expression<Func<T, bool>> criteria)
        {
            var query = _entity.Where(criteria);
            return await query.AnyAsync() ? await query.AverageAsync(expression) : 0.0;
        }

        public async Task<double> GetAverageAsync(Expression<Func<T, double>> expression)
            => await _entity.AverageAsync(expression);

        public async Task AddEntityAsync(T entity)
            => await _entity.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities)
            => await _entity.AddRangeAsync(entities);

        public Task UpdateEntityAsync(T entity)
        {
            _entity.Update(entity);
            return Task.CompletedTask;
        }

        public async Task ExecuteUpdateAsync(Expression<Func<T, bool>> filter, Expression<Func<T, T>> updateExpression)
        {
            var entities = await _entity.Where(filter).ToListAsync();
            if (!entities.Any()) return;

            var compiledUpdate = updateExpression.Compile();
            foreach (var entity in entities)
            {
                var updatedEntity = compiledUpdate(entity);
                _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            }

            await _context.SaveChangesAsync();
        }

        public Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _entity.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public Task DeleteEntityAsync(T entity)
        {
            _entity.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _entity.RemoveRange(entities);
            return Task.CompletedTask;
        }

        private async Task<(IList<T> items, int totalCount)> GetPagedDataAsync(IQueryable<T> query, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(1, pageNumber); // Ensure positive page number
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }
    }

}
