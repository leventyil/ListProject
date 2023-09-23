using ListProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ListProject.Repository
{
    public interface IRepository<T>
    {
        List<T> GetAll();
        List<T> GetAll(Expression<Func<T, bool>> expression);
        void Add(T entity);
        T Find(Guid id);
        T Get(Expression<Func<T, bool>> expression);
        void Update(T entity);
        void Delete(T entity);
        int Save();
        Task<T> FindAsync(Guid id);
        Task<T> GetAsync(Expression<Func<T, bool>> expression);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task<int> SaveAsync();
        DbSet<WantToWatchList> WantToWatchList();
        DbSet<WatchedList> WatchedList();
    }
}
