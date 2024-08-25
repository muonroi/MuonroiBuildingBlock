namespace MBuildingBlock.Entity
{
    namespace MBuildingBlock.Entity
    {
        public class MBaseIdentityDbContext<TUser, TRole> : IdentityDbContext<TUser, TRole, Guid> where TUser : IdentityUser<Guid> where TRole : IdentityRole<Guid>
        {
            private readonly IMediator _mediator;
            private IDbContextTransaction? _currentTransaction;
            public bool HasActiveTransaction => _currentTransaction != null;
            private readonly List<MEntity> _trackEntities = [];

            public IDbContextTransaction? GetCurrentTransaction()
            {
                return _currentTransaction;
            }

            public async Task<Guid> SaveEntitiesAsync(CancellationToken cancellationToken = default)
            {
                IExecutionStrategy strategy = Database.CreateExecutionStrategy();
                if (Database.IsInMemory())
                {
                    _ = await base.SaveChangesAsync(cancellationToken);
                    await DispatchDomainEventsAsync();
                    return Guid.NewGuid();
                }

                if (HasActiveTransaction)
                {
                    _ = await base.SaveChangesAsync(cancellationToken);
                    await DispatchDomainEventsAsync();
                    return _currentTransaction?.TransactionId ?? Guid.NewGuid();
                }

                return await strategy.ExecuteAsync(async () =>
                {
                    IDbContextTransaction? dbContextTransaction = await BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        _ = await base.SaveChangesAsync(cancellationToken);
                        await DispatchDomainEventsAsync();
                        await CommitTransactionAsync(dbContextTransaction!).ConfigureAwait(false);
                        return dbContextTransaction!.TransactionId;
                    }
                    catch (Exception)
                    {
                        RollbackTransaction();
                        throw;
                    }
                });
            }

            private async Task DispatchDomainEventsAsync()
            {
                IEnumerable<global::Muonroi.BuildingBlock.External.Entity.MEntity> domainEntities = _trackEntities.Where(x => x.DomainEvents != null && x.DomainEvents.Count > 0).Distinct();
                List<INotification> domainEvents = domainEntities.SelectMany(x => x.DomainEvents ?? []).ToList();
                domainEntities.ToList().ForEach(entity =>
                {
                    entity.ClearDomainEvents();
                });
                IEnumerable<Task> tasks = domainEvents.Select(async domainEvent =>
                {
                    Console.WriteLine($"Dispatching InternalEvent: {domainEvent.GetType()}");
                    await _mediator.Publish(domainEvent);
                    Console.WriteLine($"Dispatched InternalEvent: {domainEvent.GetType()}");
                });
                await Task.WhenAll(tasks);
            }

            public async Task<IDbContextTransaction?> BeginTransactionAsync()
            {
                if (_currentTransaction != null)
                {
                    return null;
                }

                _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted);
                return _currentTransaction;
            }

            public async Task CommitTransactionAsync(IDbContextTransaction transaction)
            {
                ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

                if (transaction != _currentTransaction)
                {
                    throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");
                }

                try
                {
                    _ = await SaveChangesAsync();
                    transaction.Commit();
                }
                catch
                {
                    RollbackTransaction();
                    throw;
                }
                finally
                {
                    _currentTransaction?.Dispose();
                    _currentTransaction = null;
                }
            }

            public void RollbackTransaction()
            {
                try
                {
                    _currentTransaction?.Rollback();
                }
                finally
                {
                    _currentTransaction?.Dispose();
                    _currentTransaction = null;
                }
            }

            public void TrackEntity(MEntity entity)
            {
                _trackEntities.Add(entity);
            }

            public MBaseIdentityDbContext(DbContextOptions options, IMediator mediator)
                : base(options)
            {
                _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
                Debug.WriteLine("BaseIdentityDbContext::ctor ->" + GetHashCode());
            }
        }
    }
}