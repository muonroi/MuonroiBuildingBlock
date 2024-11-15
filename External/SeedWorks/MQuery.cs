namespace Muonroi.BuildingBlock.External.SeedWorks
{
    public class MQuery<T> : IMQueries<T> where T : MEntity
    {
        protected readonly MDbContext _dbBaseContext;

        protected readonly MAuthenticateInfoContext _authContext;

        protected readonly DbSet<T> _dbSet;

        public string? CurrentUserId => _authContext?.CurrentUserGuid;

        public string? CurrentUsername => _authContext?.CurrentUsername;

        public IQueryable<T> Queryable => from m in _dbSet.AsNoTracking()
                                          where !m.IsDeleted
                                          select m;

        public MQuery(MDbContext dbContext, MAuthenticateInfoContext authContext)
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

        public virtual async Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await Queryable.Where(predicate).ToListAsync().ConfigureAwait(false);
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


        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            try
            {
                return predicate == null
                    ? await Queryable.CountAsync().ConfigureAwait(false)
                    : await Queryable.CountAsync(predicate).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MPagedResult<TDto>> GetPagedAsync<TDto>(
                    IQueryable<T> query,
                    int pageIndex,
                    int pageSize,
                    Expression<Func<T, TDto>> selector,
                    string? keyword = null,
                    Expression<Func<T, bool>>? filter = null,
                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
                    where TDto : class
        {
            try
            {
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => EF.Functions.Like(x.ToString(), $"%{keyword}%"));
                }

                int totalRowCount = await query.CountAsync().ConfigureAwait(false);

                if (orderBy != null)
                {
                    query = orderBy(query);
                }

                List<TDto> items = await query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(selector)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return new MPagedResult<TDto>
                {
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    RowCount = totalRowCount,
                    Items = items
                };
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await Queryable.FirstOrDefaultAsync(predicate).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}