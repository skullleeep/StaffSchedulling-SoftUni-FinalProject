using System.Linq.Expressions;

namespace StaffScheduling.Data.Repository.Contracts
{
    public interface IGenericRepository<TType, TId>
    {
        TType? GetById(TId id);

        Task<TType?> GetByIdAsync(TId id);

        TType? FirstOrDefault(Func<TType, bool> predicate);

        Task<TType?> FirstOrDefaultAsync(Expression<Func<TType, bool>> predicate);

        IQueryable<TType> All();

        Task<IEnumerable<TType>> GetAllNotAttachedAsync();

        IEnumerable<TType> GetAllNotAttached();

        void Add(TType item);

        Task AddAsync(TType item);

        void AddRange(TType[] items);

        Task AddRangeAsync(TType[] items);

        void Update(TType item);

        bool Delete(TId id);

        Task<bool> DeleteAsync(TId id);

        bool DeleteRange(TType[] items);
    }
}
