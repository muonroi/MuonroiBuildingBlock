namespace MuonroiBuildingBlock.Entity
{
    public class BaseDbContext : DbContext, IUnitOfWork, IDisposable
    {
        private readonly IMediator _mediator;

        private IDbContextTransaction? _currentTransaction;
        public bool HasActiveTransaction => _currentTransaction != null;
        private readonly List<EntityBase> _trackEntities = [];

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
                return _currentTransaction?.TransactionId ?? Guid.NewGuid(); // Đảm bảo không trả về giá trị null
            }

            return await strategy.ExecuteAsync(async () =>
            {
                IDbContextTransaction? dbContextTransaction = await BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    _ = await base.SaveChangesAsync(cancellationToken);
                    await DispatchDomainEventsAsync();
                    await CommitTransactionAsync(dbContextTransaction!).ConfigureAwait(false); // Thêm ! để tránh cảnh báo null
                    return dbContextTransaction!.TransactionId; // Thêm ! để tránh cảnh báo null
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
            IEnumerable<EntityBase> domainEntities = _trackEntities.Where(x => x.DomainEvents != null && x.DomainEvents.Count > 0).Distinct();
            List<INotification> domainEvents = domainEntities.SelectMany(x => x.DomainEvents ?? []).ToList();
            domainEntities.ToList().ForEach(entity =>
            {
                entity.ClearDomainEvents();
            });
            IEnumerable<Task> tasks = domainEvents.Select(async domainEvent =>
            {
                Console.WriteLine($"Dispatching InternalEvent: {domainEvent.GetType()}");
                if (_mediator is not null)
                {
                    await _mediator.Publish(domainEvent);
                }
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
        public void TrackEntity(EntityBase entity)
        {
            _trackEntities.Add(entity);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            _ = builder.ApplyConfiguration(new AppUserConfiguration());
            _ = builder.ApplyConfiguration(new AppUserAccountConfiguration());
            _ = builder.ApplyConfiguration(new AppUserLoginConfiguration());
            _ = builder.ApplyConfiguration(new AppUserRoleConfiguration());
            _ = builder.ApplyConfiguration(new AppLanguageConfiguration());
            _ = builder.ApplyConfiguration(new AppPermissionConfiguration());
            _ = builder.ApplyConfiguration(new AppRoleConfiguration());
            _ = builder.ApplyConfiguration(new AppUserTokenConfiguration());
            _ = builder.ApplyConfiguration(new AppUserLoginAttemptConfiguration());
        }

        // Hàm khởi tạo của lớp
        public BaseDbContext(DbContextOptions options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
            Debug.WriteLine("BaseDbContext::ctor ->" + GetHashCode());
        }
    }
}