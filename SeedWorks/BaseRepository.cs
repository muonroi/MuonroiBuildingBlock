namespace MuonroiBuildingBlock.SeedWorks
{
    public class BaseRepository<T> : IBaseRepository<T> where T : EntityBase
    {
        private readonly AuthContext _authContext;
        protected readonly BaseDbContext _dbBaseContext;
        protected readonly DbSet<T> _dbSet;

        public int? CurrentUserId => _authContext.AuthInfoContext?.CurrentUserId;
        public string? CurrentUsername => _authContext.AuthInfoContext?.CurrentUsername;
        public IUnitOfWork UnitOfWork => _dbBaseContext;
        protected IQueryable<T> Queryable => _dbSet.Where(m => !m.IsDeleted);

        public BaseRepository(BaseDbContext dbContext, AuthContext authContext)
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
                newEntity.CreatedUserId = _authContext.AuthInfoContext?.CurrentUserId;
                newEntity.CreatedUserName = _authContext.AuthInfoContext?.CurrentUsername;
                newEntity.EntityId = Guid.NewGuid();
                newEntity.AddDomainEvent(new EntityCreatedEvent<T>(newEntity));
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
                deleteEntity.DeletedUserId = _authContext.AuthInfoContext?.CurrentUserId;
                deleteEntity.DeletedUserName = _authContext.AuthInfoContext?.CurrentUsername;
                deleteEntity.AddDomainEvent(new EntityDeletedEvent<T>(deleteEntity));
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
                updateEntity.UpdatedUserId = _authContext.AuthInfoContext?.CurrentUserId;
                updateEntity.UpdatedUserName = _authContext.AuthInfoContext?.CurrentUsername;
                updateEntity.AddDomainEvent(new EntityChangedEvent<T>(updateEntity));
                _dbBaseContext.TrackEntity(updateEntity);
                return _dbSet.Update(updateEntity).Entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task ExecuteTransactionAsync(Func<Task<VoidMethodResult>> action)
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
                    VoidMethodResult result = await action().ConfigureAwait(false);

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
    }
}