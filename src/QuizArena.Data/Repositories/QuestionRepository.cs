using Microsoft.EntityFrameworkCore;
using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(QuizArenaDbContext context) : base(context)
    {
    }

    // Soru + şıkları + quizi (quizin sahibini kontrol etmek için lazım)
    public async Task<Question?> GetWithOptionsAsync(int questionId)
    {
        var question = await _context.Questions
            .Include(q => q.Quiz)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question != null)
        {
            question.Options = question.Options.OrderBy(o => o.OrderNo).ToList();
        }

        return question;
    }

    public async Task<List<Question>> GetByQuizAsync(int quizId)
    {
        return await _context.Questions
            .Where(q => q.QuizId == quizId)
            .OrderBy(q => q.OrderNo)
            .ToListAsync();
    }

    public void RemoveOptions(List<AnswerOption> options)
    {
        _context.AnswerOptions.RemoveRange(options);
    }
}
