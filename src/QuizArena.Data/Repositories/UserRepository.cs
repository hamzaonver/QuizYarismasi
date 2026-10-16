using Microsoft.EntityFrameworkCore;
using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(QuizArenaDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}
