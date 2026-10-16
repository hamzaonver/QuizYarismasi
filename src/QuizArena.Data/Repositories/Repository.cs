using Microsoft.EntityFrameworkCore;
using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly QuizArenaDbContext _context;

    public Repository(QuizArenaDbContext context)
    {
        _context = context;
    }

    // SELECT * FROM Tablo WHERE Id = @id
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
    }

    // SELECT * FROM Tablo
    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    // INSERT INTO Tablo ...
    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    // UPDATE Tablo SET ... WHERE Id = @id
    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    // DELETE FROM Tablo WHERE Id = @id
    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    // Yapılan değişiklikleri veritabanına yazar
    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
