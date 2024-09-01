namespace Muonroi.BuildingBlock.External.Entity
{
    public class MDbContext : DbContext, IMUnitOfWork, IDisposable, IIdentityAuth
    {
        private readonly IMediator _mediator;

        private IDbContextTransaction? _currentTransaction;
        private readonly List<MEntity> _trackEntities = [];
        public bool HasActiveTransaction => _currentTransaction != null;

        public DbSet<MUserAccount> UserAccounts { get; set; }
        public DbSet<MUser> Users { get; set; }
        public DbSet<MRole> Roles { get; set; }
        public DbSet<MPermission> Permissions { get; set; }
        public DbSet<MUserRole> UserRoles { get; set; }
        public DbSet<MLanguage> Languages { get; set; }
        public DbSet<MUserLogin> UserLogins { get; set; }
        public DbSet<MUserToken> UserTokens { get; set; }
        public DbSet<MUserLoginAttempt> MUserLoginAttempts { get; set; }

        public MDbContext(DbContextOptions options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
            Debug.WriteLine("BaseDbContext::ctor ->" + GetHashCode());
        }

        public IDbContextTransaction? GetCurrentTransaction()
        {
            return _currentTransaction;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public async Task<Guid> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
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

        private void UpdateTimestamps()
        {
            IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> modifiedEntries = ChangeTracker
                .Entries()
                .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? item in modifiedEntries)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is MEntity addedEntity)
                        {
                            addedEntity.CreatedDateTS = DateTime.UtcNow.GetTimeStamp();
                            addedEntity.CreationTime = DateTime.UtcNow;
                            item.State = EntityState.Added;
                        }
                        break;

                    case EntityState.Modified:
                        Entry(item.Entity).Property("Id").IsModified = false;
                        if (item.Entity is MEntity modifiedEntity)
                        {
                            modifiedEntity.LastModificationTime = DateTime.UtcNow;
                            modifiedEntity.LastModificationTimeTs = DateTime.UtcNow.GetTimeStamp();
                            item.State = EntityState.Modified;
                        }
                        break;

                    case EntityState.Deleted:
                        if (item.Entity is MEntity deletedEntity)
                        {
                            deletedEntity.IsDeleted = true;
                            deletedEntity.DeletionTime = DateTime.UtcNow;
                            deletedEntity.DeletedDateTS = DateTime.UtcNow.GetTimeStamp();
                            item.State = EntityState.Modified;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private async Task DispatchDomainEventsAsync()
        {
            IEnumerable<MEntity> domainEntities = _trackEntities
                .Where(x => x.DomainEvents != null && x.DomainEvents.Count > 0)
                .Distinct();

            List<INotification> domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents ?? new List<INotification>())
                .ToList();

            domainEntities.ToList().ForEach(entity => entity.ClearDomainEvents());

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            _ = builder.ApplyConfiguration(new MUserConfiguration());
            _ = builder.ApplyConfiguration(new MUserAccountConfiguration());
            _ = builder.ApplyConfiguration(new MUserLoginConfiguration());
            _ = builder.ApplyConfiguration(new MUserRoleConfiguration());
            _ = builder.ApplyConfiguration(new MLanguageConfiguration());
            _ = builder.ApplyConfiguration(new MPermissionConfiguration());
            _ = builder.ApplyConfiguration(new MRoleConfiguration());
            _ = builder.ApplyConfiguration(new MUserTokenConfiguration());
            _ = builder.ApplyConfiguration(new MUserLoginAttemptConfiguration());
        }
    }
}