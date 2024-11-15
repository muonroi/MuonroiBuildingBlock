
namespace Muonroi.BuildingBlock.External.SeedWorks
{
    public class MRepository<T> : IMRepository<T> where T : MEntity
    {
        private readonly MAuthenticateInfoContext _authContext;
        protected readonly MDbContext _dbBaseContext;
        protected readonly DbSet<T> _dbSet;

        public string? CurrentUserId => _authContext?.CurrentUserGuid;
        public string? CurrentUsername => _authContext?.CurrentUsername;
        public IMUnitOfWork UnitOfWork => _dbBaseContext;
        protected IQueryable<T> Queryable => _dbSet.Where(m => !m.IsDeleted);

        public MRepository(MDbContext dbContext, MAuthenticateInfoContext authContext)
        {
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));
            _dbBaseContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbBaseContext.Set<T>();
        }

        public virtual async Task<int> AddBatchAsync(IEnumerable<T> newEntities)
        {
            if (newEntities == null || !newEntities.Any())
            {
                return 0;
            }

            try
            {
                DateTime utcNow = DateTime.UtcNow;
                foreach (T entity in newEntities)
                {
                    entity.CreatedDateTS = utcNow.GetTimeStamp(true);
                    entity.LastModificationTimeTs = utcNow.GetTimeStamp(true);
                    entity.CreatorUserId = string.IsNullOrEmpty(_authContext?.CurrentUserGuid)
                        ? Guid.Empty
                        : Guid.Parse(_authContext?.CurrentUserGuid ?? Guid.Empty.ToString());
                    entity.EntityId = Guid.NewGuid();
                    entity.AddDomainEvent(new MEntityCreatedEvent<T>(entity));
                    _dbBaseContext.TrackEntity(entity);
                }

                await _dbSet.AddRangeAsync(newEntities).ConfigureAwait(false);
                return await _dbBaseContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<int> UpdateBatchAsync(Expression<Func<T, bool>> predicate, Action<T> updateAction)
        {
            try
            {
                List<T> entities = await Queryable.Where(predicate).ToListAsync().ConfigureAwait(false);
                if (entities.Count == 0)
                {
                    return 0;
                }

                foreach (T? entity in entities)
                {
                    updateAction(entity);
                    entity.LastModificationTimeTs = DateTime.UtcNow.GetTimeStamp(true);
                    entity.LastModificationUserId = string.IsNullOrEmpty(_authContext?.CurrentUserGuid)
                        ? null
                        : Guid.Parse(_authContext?.CurrentUserGuid ?? Guid.Empty.ToString());
                    entity.AddDomainEvent(new MEntityChangedEvent<T>(entity));
                    _dbBaseContext.TrackEntity(entity);
                }

                return await _dbBaseContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<int> DeleteBatchAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                List<T> entities = await Queryable.Where(predicate).ToListAsync().ConfigureAwait(false);
                if (entities.Count == 0)
                {
                    return 0;
                }

                foreach (T? entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.DeletedDateTS = DateTime.UtcNow.GetTimeStamp(true);
                    entity.DeletedUserId = string.IsNullOrEmpty(_authContext?.CurrentUserGuid)
                        ? null
                        : Guid.Parse(_authContext?.CurrentUserGuid ?? Guid.Empty.ToString());
                    entity.AddDomainEvent(new MEntityDeletedEvent<T>(entity));
                    _dbBaseContext.TrackEntity(entity);
                }

                return await _dbBaseContext.SaveChangesAsync().ConfigureAwait(false);
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
                newEntity.LastModificationTimeTs = utcNow.GetTimeStamp(true);
                newEntity.CreatorUserId = string.IsNullOrEmpty(_authContext?.CurrentUserGuid) ? Guid.Empty : Guid.Parse(_authContext?.CurrentUserGuid ?? Guid.Empty.ToString());
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

        public virtual Task<bool> DeleteAsync(T deleteEntity)
        {
            try
            {
                deleteEntity.IsDeleted = true;
                deleteEntity.DeletedDateTS = DateTime.UtcNow.GetTimeStamp(true);
                deleteEntity.DeletedUserId = string.IsNullOrEmpty(_authContext?.CurrentUserGuid) ? null : Guid.Parse(_authContext?.CurrentUserGuid ?? Guid.Empty.ToString());
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
                updateEntity.LastModificationTimeTs = DateTime.UtcNow.GetTimeStamp(true);
                updateEntity.LastModificationUserId = string.IsNullOrEmpty(_authContext?.CurrentUserGuid) ? null : Guid.Parse(_authContext?.CurrentUserGuid ?? Guid.Empty.ToString());
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


        public async Task<bool> SoftRestoreAsync(T entity)
        {
            try
            {
                if (entity.IsDeleted)
                {
                    entity.IsDeleted = false;
                    entity.DeletedDateTS = null;
                    entity.DeletedUserId = null;
                    entity.LastModificationTimeTs = DateTime.UtcNow.GetTimeStamp(true);
                    entity.LastModificationUserId = string.IsNullOrEmpty(_authContext?.CurrentUserGuid)
                        ? null
                        : Guid.Parse(_authContext?.CurrentUserGuid ?? Guid.Empty.ToString());
                    entity.AddDomainEvent(new MEntityChangedEvent<T>(entity));
                    _dbBaseContext.TrackEntity(entity);

                    return await _dbBaseContext.SaveChangesAsync().ConfigureAwait(false) > 0;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            try
            {
                await _dbBaseContext.BulkInsertAsync(entities.ToList()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Bulk insert failed.", ex);
            }
        }

    }
}