using Microsoft.EntityFrameworkCore;
using QuizArena.Entities;
using QuizArena.Entities.Enums;

namespace QuizArena.Data.Repositories;

public class SessionRepository : Repository<QuizSession>, ISessionRepository
{
    public SessionRepository(QuizArenaDbContext context) : base(context)
    {
    }

    public async Task<QuizSession?> GetWithDetailsAsync(int sessionId)
    {
        var session = await _context.QuizSessions
            .Include(s => s.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Options)
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session != null)
        {
            // Soruları ve şıkları sıraya koy
            session.Quiz.Questions = session.Quiz.Questions.OrderBy(q => q.OrderNo).ToList();
            foreach (var question in session.Quiz.Questions)
            {
                question.Options = question.Options.OrderBy(o => o.OrderNo).ToList();
            }
        }

        return session;
    }

    public async Task<QuizSession?> GetByJoinCodeAsync(string joinCode)
    {
        return await _context.QuizSessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.JoinCode == joinCode);
    }

    public async Task<bool> JoinCodeExistsAsync(string joinCode)
    {
        return await _context.QuizSessions.AnyAsync(s => s.JoinCode == joinCode);
    }

    public async Task<List<QuizSession>> GetByHostAsync(int hostId)
    {
        return await _context.QuizSessions
            .Include(s => s.Quiz)
            .Include(s => s.Participants)
            .Where(s => s.HostId == hostId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<QuizSession>> GetByParticipantUserAsync(int userId)
    {
        return await _context.QuizSessions
            .Include(s => s.Quiz)
            .Include(s => s.Participants)
            .Where(s => s.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<QuizSession?> GetOpenSessionForQuizAsync(int quizId)
    {
        return await _context.QuizSessions
            .FirstOrDefaultAsync(s => s.QuizId == quizId && s.Status != SessionStatus.Finished);
    }
}
