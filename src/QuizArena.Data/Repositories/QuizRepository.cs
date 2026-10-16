using Microsoft.EntityFrameworkCore;
using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

public class QuizRepository : Repository<Quiz>, IQuizRepository
{
    public QuizRepository(QuizArenaDbContext context) : base(context)
    {
    }

    // Öğretmenin quizleri (soru ve oturum sayılarını göstermek için Include ediyorum)
    public async Task<List<Quiz>> GetByTeacherAsync(int teacherId)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .Include(q => q.Sessions)
            .Where(q => q.TeacherId == teacherId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    // Quiz + soruları + şıkları
    public async Task<Quiz?> GetWithQuestionsAsync(int quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(s => s.Options)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz != null)
        {
            // Soruları ve şıkları sıraya koy
            quiz.Questions = quiz.Questions.OrderBy(s => s.OrderNo).ToList();
            foreach (var question in quiz.Questions)
            {
                question.Options = question.Options.OrderBy(o => o.OrderNo).ToList();
            }
        }

        return quiz;
    }

    // Bu quiz ile hiç oturum açıldı mı?
    public async Task<bool> IsUsedInSessionAsync(int quizId)
    {
        return await _context.QuizSessions.AnyAsync(s => s.QuizId == quizId);
    }
}
