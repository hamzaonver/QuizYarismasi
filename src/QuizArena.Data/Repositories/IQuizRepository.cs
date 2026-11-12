using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<List<Quiz>> GetByTeacherAsync(int teacherId);
    Task<Quiz?> GetWithQuestionsAsync(int quizId);
    Task<bool> IsUsedInSessionAsync(int quizId);
    Task<bool> HasOpenSessionAsync(int quizId); // Hafta 8
}
