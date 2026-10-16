using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

// Bütün tablolar için ortak CRUD işlemleri
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task SaveAsync();
}
