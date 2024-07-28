namespace MBuildingBlock.Entity
{
    namespace MBuildingBlock.Entity
    {
        // Lớp cơ sở cho IdentityDbContext với các chức năng liên quan đến Identity
        public class MBaseIdentityDbContext<TUser, TRole> : IdentityDbContext<TUser, TRole, Guid> where TUser : IdentityUser<Guid> where TRole : IdentityRole<Guid>
        {
            private readonly IMediator _mediator;
            private IDbContextTransaction? _currentTransaction;
            public bool HasActiveTransaction => _currentTransaction != null;
            private readonly List<global::MBuildingBlock.External.Entity.MEntity> _trackEntities = [];

            // Phương thức lấy giao dịch hiện tại
            public IDbContextTransaction? GetCurrentTransaction()
            {
                return _currentTransaction;
            }

            // Phương thức lưu các thay đổi vào cơ sở dữ liệu và phát tán các sự kiện miền
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

            // Phương thức phát tán các sự kiện miền
            private async Task DispatchDomainEventsAsync()
            {
                IEnumerable<global::MBuildingBlock.External.Entity.MEntity> domainEntities = _trackEntities.Where(x => x.DomainEvents != null && x.DomainEvents.Count > 0).Distinct();
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

            // Phương thức bắt đầu giao dịch
            public async Task<IDbContextTransaction?> BeginTransactionAsync()
            {
                if (_currentTransaction != null)
                {
                    return null;
                }

                _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted);
                return _currentTransaction;
            }

            // Phương thức cam kết giao dịch
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

            // Phương thức hủy bỏ giao dịch
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

            // Phương thức theo dõi thực thể
            public void TrackEntity(global::MBuildingBlock.External.Entity.MEntity entity)
            {
                _trackEntities.Add(entity);
            }

            // Hàm khởi tạo của lớp
            public MBaseIdentityDbContext(DbContextOptions options, IMediator mediator)
                : base(options)
            {
                _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
                Debug.WriteLine("BaseIdentityDbContext::ctor ->" + GetHashCode());
            }
        }
    }
}