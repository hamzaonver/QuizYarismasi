using QuizArena.Entities;

namespace QuizArena.Data.Repositories;

public interface ISessionRepository : IRepository<QuizSession>
{
    // Oturum + quiz + sorular + şıklar + katılımcılar
    Task<QuizSession?> GetWithDetailsAsync(int sessionId);

    // Öğrencinin girdiği katılım koduna göre oturumu bulur
    Task<QuizSession?> GetByJoinCodeAsync(string joinCode);

    Task<bool> JoinCodeExistsAsync(string joinCode);

    // Öğretmenin açtığı oturumlar
    Task<List<QuizSession>> GetByHostAsync(int hostId);

    // Öğrencinin katıldığı oturumlar
    Task<List<QuizSession>> GetByParticipantUserAsync(int userId);

    // Bu quiz için bitmemiş (bekleyen veya devam eden) oturum var mı?
    Task<QuizSession?> GetOpenSessionForQuizAsync(int quizId);
}
