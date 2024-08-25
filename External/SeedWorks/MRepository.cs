namespace Muonroi.BuildingBlock.External.SeedWorks
{
    public class MRepository<T> : IMRepository<T> where T : MEntity
    {
        private readonly MAuthenticateInfoContext _authContext;
        protected readonly MDbContext _dbBaseContext;
        protected readonly DbSet<T> _dbSet;

        public int? CurrentUserId => _authContext?.CurrentUserId;
        public string? CurrentUsername => _authContext?.CurrentUsername;
        public IMUnitOfWork UnitOfWork => _dbBaseContext;
        protected IQueryable<T> Queryable => _dbSet.Where(m => !m.IsDeleted);

        public MRepository(MDbContext dbContext, MAuthenticateInfoContext authContext)
        {
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));
            _dbBaseContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbBaseContext.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await Queryable.SingleOrDefaultAsync(c => c.Id == id).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<T?> GetByGuidAsync(Guid guid)
        {
            try
            {
                return await Queryable.SingleOrDefaultAsync(c => c.EntityId == guid).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual T Add(T newEntity)
        {
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                newEntity.CreatedDateTS = utcNow.GetTimeStamp(true);
                newEntity.UpdatedDateTS = utcNow.GetTimeStamp(true);
                newEntity.CreatedUserId = _authContext?.CurrentUserId;
                newEntity.CreatedUserName = _authContext?.CurrentUsername;
                newEntity.EntityId = Guid.NewGuid();
                newEntity.AddDomainEvent(new MEntityCreatedEvent<T>(newEntity));
                _dbBaseContext.TrackEntity(newEntity);
                return _dbSet.Add(newEntity).Entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<bool> AnyAsync(int id)
        {
            try
            {
                return await Queryable.AnyAsync(c => c.Id == id).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AnyGuidAsync(Guid guid)
        {
            try
            {
                return await Queryable.AnyAsync(c => c.EntityId == guid).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual Task<bool> DeleteAsync(T deleteEntity)
        {
            try
            {
                deleteEntity.IsDeleted = true;
                deleteEntity.DeletedDateTS = DateTime.UtcNow.GetTimeStamp(true);
                deleteEntity.DeletedUserId = _authContext?.CurrentUserId;
                deleteEntity.DeletedUserName = _authContext?.CurrentUsername;
                deleteEntity.AddDomainEvent(new MEntityDeletedEvent<T>(deleteEntity));
                _dbBaseContext.TrackEntity(deleteEntity);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual T Update(T updateEntity)
        {
            try
            {
                updateEntity.UpdatedDateTS = DateTime.UtcNow.GetTimeStamp(true);
                updateEntity.UpdatedUserId = _authContext?.CurrentUserId;
                updateEntity.UpdatedUserName = _authContext?.CurrentUsername;
                updateEntity.AddDomainEvent(new MEntityChangedEvent<T>(updateEntity));
                _dbBaseContext.TrackEntity(updateEntity);
                return _dbSet.Update(updateEntity).Entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task ExecuteTransactionAsync(Func<Task<MVoidMethodResult>> action)
        {
            if (_dbBaseContext.Database.IsInMemory() || _dbBaseContext.HasActiveTransaction)
            {
                _ = await action().ConfigureAwait(false);
                return;
            }

            IExecutionStrategy strategy = _dbBaseContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                IDbContextTransaction? transaction = null;
                try
                {
                    transaction = await _dbBaseContext.BeginTransactionAsync().ConfigureAwait(false);
                    MVoidMethodResult result = await action().ConfigureAwait(false);

                    if (result?.IsOK ?? false)
                    {
                        transaction?.Commit();
                    }
                    else
                    {
                        transaction?.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                }
            }).ConfigureAwait(false);
        }

        public async Task RollbackTransactionAsync()
        {
            IDbContextTransaction? transaction = _dbBaseContext.Database.CurrentTransaction;
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
        }
    }
}