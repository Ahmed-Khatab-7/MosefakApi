namespace MosefakApp.Infrastructure.Unit
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new(); // Thread-safe dictionary
        private readonly ConcurrentDictionary<Type, object> _customRepositories = new(); // Thread-safe dictionary
        private IDbContextTransaction? _transaction;
        private bool _disposed;
        private readonly ILoggerService _logger;
        public UnitOfWork(AppDbContext context, IEnumerable<object> customRepositories = null, ILoggerService logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (customRepositories != null)
            {
                _logger.LogInfo("Total repositories received: {Count}", customRepositories.Count());

                foreach (var repo in customRepositories)
                {
                    if (repo == null)
                    {
                        throw new ArgumentException("Custom repository cannot be null.", nameof(customRepositories));
                    }

                    _logger.LogInfo("Processing repository: {RepositoryType}", repo.GetType().Name);

                    var interfaces = repo.GetType().GetInterfaces();
                    foreach (var interfaceType in interfaces)
                    {
                        _logger.LogDebug("Checking interface: {InterfaceType}", interfaceType.Name);

                        if (interfaceType == typeof(IDoctorRepositoryAsync)) // Explicitly check for this type
                        {
                            _customRepositories.TryAdd(interfaceType, repo);
                            _logger.LogInfo("Registered repository under interface: {InterfaceType}", interfaceType.Name);
                        }
                        if (interfaceType == typeof(IAppointmentRepositoryAsync)) // Explicitly check for this type
                        {
                            _customRepositories.TryAdd(interfaceType, repo);
                            _logger.LogInfo("Registered repository under interface: {InterfaceType}", interfaceType.Name);
                        }
                        if (interfaceType == typeof(IClinicRepository)) // Explicitly check for this type
                        {
                            _customRepositories.TryAdd(interfaceType, repo);
                            _logger.LogInfo("Registered repository under interface: {InterfaceType}", interfaceType.Name);
                        }
                        if (interfaceType == typeof(IPatientRepositoryAsync)) // Explicitly check for this type
                        {
                            _customRepositories.TryAdd(interfaceType, repo);
                            _logger.LogInfo("Registered repository under interface: {InterfaceType}", interfaceType.Name);
                        }
                        if (interfaceType == typeof(IUserRepository)) // Explicitly check for this type
                        {
                            _customRepositories.TryAdd(interfaceType, repo);
                            _logger.LogInfo("Registered repository under interface: {InterfaceType}", interfaceType.Name);
                        }
                    }

                    _customRepositories.TryAdd(repo.GetType(), repo);
                    _logger.LogInfo("Registered repository under concrete type: {ConcreteType}", repo.GetType().Name);
                }
            }
            else
            {
                _logger.LogWarning("No custom repositories were passed to UnitOfWork.");
            }

            _logger.LogInfo("Total registered repositories in _customRepositories: {Count}", _customRepositories.Count);
        }



        // ✅ Generic repository for standard CRUD (thread-safe and cached)
        public IGenericRepositoryAsync<T> Repository<T>() where T : class
        {
            return (IGenericRepositoryAsync<T>)_repositories.GetOrAdd(
                typeof(T),
                _ => new GenericRepositoryAsync<T>(_context));
        }

        public TRepository GetCustomRepository<TRepository>() where TRepository : class
        {
            var repositoryType = typeof(TRepository);
            _logger.LogInfo("Attempting to retrieve repository of type: {RepositoryType}", repositoryType.Name);

            // Log available repositories
            if (!_customRepositories.Any())
            {
                _logger.LogWarning("No custom repositories are registered.");
            }
            else
            {
                _logger.LogInfo("Available repositories: {RegisteredTypes}",
                    string.Join(", ", _customRepositories.Keys.Select(k => k.Name)));
            }

            if (_customRepositories.TryGetValue(repositoryType, out var repository))
            {
                _logger.LogInfo("Found repository of type: {RepositoryType}", repositoryType.Name);
                return (TRepository)repository;
            }

            _logger.LogError("Repository of type {RepositoryType} is not registered.", repositoryType.Name);
            throw new InvalidOperationException($"Repository of type {repositoryType.Name} is not registered. Ensure it is provided in the UnitOfWork constructor.");
        }

        public IExecutionStrategy CreateExecutionStrategy() 
        {
            return _context.Database.CreateExecutionStrategy();
        }

        // ✅ Commit changes with better error handling and cancellation support
        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Concurrency conflict occurred during save.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database update failed beacuse {ex.Message}.");
            }
        }

        // ✅ Begin a transaction with better null and state checking
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            if (_context.Database.CurrentTransaction != null)
            {
                throw new InvalidOperationException("An external transaction is already active on the context.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        // ✅ Commit transaction with proper disposal and error handling
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to commit transaction.", ex);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        // ✅ Rollback transaction with proper disposal and error handling
        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to rollback transaction.", ex);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        // ✅ Dispose pattern implementation for proper resource cleanup
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _context?.Dispose();
                _transaction?.Dispose();
            }

            _disposed = true;
        }

        // ✅ Async dispose for compatibility with IAsyncDisposable (optional, if needed)
        public async ValueTask DisposeAsync()
        {
            Dispose(true);
            await _context.DisposeAsync();
        }
    }

}
