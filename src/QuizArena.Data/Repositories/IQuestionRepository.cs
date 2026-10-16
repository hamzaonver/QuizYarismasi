using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

public interface IQuestionRepository : IRepository<Question>
{
    Task<Question?> GetWithOptionsAsync(int questionId);
    Task<List<Question>> GetByQuizAsync(int quizId);
    void RemoveOptions(List<AnswerOption> options);
}
