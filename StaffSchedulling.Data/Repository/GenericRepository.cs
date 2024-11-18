using Microsoft.EntityFrameworkCore;
using StaffScheduling.Data.Repository.Contracts;
using System.Linq.Expressions;

namespace StaffScheduling.Data.Repository
{
    public class GenericRepository<TType, TId> : IGenericRepository<TType, TId>
        where TType : class
        where TId : notnull
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<TType> _dbSet;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TType>();
        }

        public void Add(TType item)
        {
            _dbSet.Add(item);
        }

        public async Task AddAsync(TType item)
        {
            await _dbSet.AddAsync(item);
        }

        public void AddRange(TType[] items)
        {
            _dbSet.AddRange(items);
        }

        public void Update(TType item)
        {
            var entry = _dbContext.Entry(item);
            entry.State = EntityState.Modified;
        }

        public async Task AddRangeAsync(TType[] items)
        {
            await _dbSet.AddRangeAsync(items);
        }

        public bool Delete(TId id)
        {
            var entity = GetById(id);
            if (entity == null)
            {
                return false;
            }

            _dbSet.Remove(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(TId id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return false;
            }

            _dbSet.Remove(entity);
            return true;
        }

        public bool DeleteRange(TType[] items)
        {
            try
            {
                _dbSet.RemoveRange(items);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public TType? FirstOrDefault(Func<TType, bool> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public async Task<TType?> FirstOrDefaultAsync(Expression<Func<TType, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public IQueryable<TType> All()
        {
            return _dbSet.AsQueryable();
        }

        public IEnumerable<TType> GetAllNotAttached()
        {
            return _dbSet.ToList();
        }

        public async Task<IEnumerable<TType>> GetAllNotAttachedAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public TType? GetById(TId id)
        {
            return _dbSet.Find(id);
        }

        public async Task<TType?> GetByIdAsync(TId id)
        {
            return await _dbSet.FindAsync(id);
        }

        /*        public int Save()
                {
                    return _dbContext.SaveChanges();
                }

                public async Task<int> SaveAsync()
                {
                    return await _dbContext.SaveChangesAsync();
                }*/
    }
}
