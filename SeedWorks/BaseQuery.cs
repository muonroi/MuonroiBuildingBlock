namespace MuonroiBuildingBlock.SeedWorks
{
    public class BaseQuery<T> : IBaseQueries<T> where T : EntityBase
    {
        protected readonly BaseDbContext _dbBaseContext;

        private readonly AuthContext _authContext;

        protected readonly DbSet<T> _dbSet;

        public int? CurrentUserId => _authContext.AuthInfoContext?.CurrentUserId;

        public string? CurrentUsername => _authContext.AuthInfoContext?.CurrentUsername;

        public IQueryable<T> Queryable => from m in _dbSet.AsNoTracking()
                                          where !m.IsDeleted
                                          select m;

        public BaseQuery(BaseDbContext dbContext, AuthContext authContext)
        {
            _dbBaseContext = dbContext;
            _authContext = authContext;
            _dbSet = _dbBaseContext.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.AsNoTracking().SingleOrDefaultAsync((T c) => c.Id == id && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
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
                return await _dbSet.AsNoTracking().SingleOrDefaultAsync((T c) => c.EntityId == guid && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
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

        public virtual async Task<PagingItems<T>?> GetAllAsync(int page, int pageSize)
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

        public async Task<PagingItems<T>> GetListPaging(IQueryable<T> query, int page, int pageSize)
        {
            PagingItems<T> pagingItemsDTO = new();
            PagingItems<T> pagingItemsDTO2 = pagingItemsDTO;
            pagingItemsDTO2.Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            PagingItems<T> pagingItemsDTO3 = pagingItemsDTO;
            PagingInfo pagingInfoDTO = new()
            {
                Page = page,
                PageSize = pageSize
            };
            PagingInfo pagingInfoDTO2 = pagingInfoDTO;
            pagingInfoDTO2.TotalItems = await query.CountAsync().ConfigureAwait(continueOnCapturedContext: false);
            pagingItemsDTO3.PagingInfo = pagingInfoDTO;
            return pagingItemsDTO;
        }
    }
}