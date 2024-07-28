namespace MBuildingBlock.External.SeedWorks
{
    public class MQuery<T> : IMQueries<T> where T : MEntity
    {
        protected readonly MDbContext _dbBaseContext;

        protected readonly MAuthInfoContext _authContext;

        protected readonly DbSet<T> _dbSet;

        public int? CurrentUserId => _authContext?.CurrentUserId;

        public string? CurrentUsername => _authContext?.CurrentUsername;

        public IQueryable<T> Queryable => from m in _dbSet.AsNoTracking()
                                          where !m.IsDeleted
                                          select m;

        public MQuery(MDbContext dbContext, MAuthInfoContext authContext)
        {
            _dbBaseContext = dbContext;
            _authContext = authContext;
            _dbSet = _dbBaseContext.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.AsNoTracking().SingleOrDefaultAsync((c) => c.Id == id && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<T?> GetByGuidAsync(Guid guid)
        {
            try
            {
                return await _dbSet.AsNoTracking().SingleOrDefaultAsync((c) => c.EntityId == guid && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<List<T>?> GetAllAsync(int? siteId = null)
        {
            try
            {
                return await (from c in _dbSet.AsNoTracking()
                              where !c.IsDeleted
                              select c).ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<MPagedResult<T>?> GetAllAsync(int page, int pageSize)
        {
            try
            {
                IOrderedQueryable<T> query = from c in _dbSet.AsNoTracking()
                                             where !c.IsDeleted
                                             select c into m
                                             orderby m.Id descending
                                             select m;
                return await GetListPaging(query, page, pageSize);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MPagedResult<T>> GetListPaging(IQueryable<T> query, int page, int pageSize)
        {
            int totalItems = await query.CountAsync().ConfigureAwait(false);
            List<T> items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new MPagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = totalItems,
                Items = items
            };
        }
    }
}