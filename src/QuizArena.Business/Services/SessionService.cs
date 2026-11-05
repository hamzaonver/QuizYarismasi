using QuizArena.Business.Common;
using QuizArena.Business.Dtos;
using QuizArena.Business.Validation;
using QuizArena.Data.Repositories;
using QuizArena.Entities;
using QuizArena.Entities.Enums;

namespace QuizArena.Business.Services;

// Canlı Oturum ve Katılım modülü (Hafta 7)
// Oturumun durumu: Waiting (bekleme salonu) -> Active (sorular soruluyor) -> Finished (bitti)
public class SessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IQuizRepository _quizRepository;

    public SessionService(ISessionRepository sessionRepository, IQuizRepository quizRepository)
    {
        _sessionRepository = sessionRepository;
        _quizRepository = quizRepository;
    }

    // ==================== ÖĞRETMEN ====================

    public async Task<List<SessionListDto>> GetTeacherSessionsAsync(int teacherId)
    {
        var sessions = await _sessionRepository.GetByHostAsync(teacherId);

        var list = new List<SessionListDto>();
        foreach (var s in sessions)
        {
            list.Add(new SessionListDto
            {
                Id = s.Id,
                QuizTitle = s.Quiz.Title,
                JoinCode = s.JoinCode,
                Status = s.Status,
                ParticipantCount = s.Participants.Count,
                CreatedAt = s.CreatedAt
            });
        }
        return list;
    }

    // Yayındaki bir quiz için oturum açar ve katılım kodu üretir
    public async Task<Result<int>> CreateAsync(int quizId, int teacherId)
    {
        var quiz = await _quizRepository.GetWithQuestionsAsync(quizId);
        if (quiz == null)
            return Result<int>.Fail("Quiz bulunamadı.", ErrorType.NotFound);
        if (quiz.TeacherId != teacherId)
            return Result<int>.Fail("Bu quiz size ait değil.", ErrorType.Forbidden);

        // Kural: sadece yayındaki ve sorusu olan quizle oturum açılabilir
        if (!quiz.IsPublished)
            return Result<int>.Fail("Oturum açmak için önce quizi yayınlamalısınız.");
        if (quiz.Questions.Count == 0)
            return Result<int>.Fail("Soru içermeyen quizle oturum açılamaz.");

        // Kural: aynı quiz için zaten bitmemiş bir oturum varsa yenisini açma, ona yönlendir
        var openSession = await _sessionRepository.GetOpenSessionForQuizAsync(quizId);
        if (openSession != null)
            return Result<int>.Ok(openSession.Id);

        var session = new QuizSession
        {
            QuizId = quizId,
            HostId = teacherId,
            JoinCode = await CreateUniqueJoinCodeAsync(),
            Status = SessionStatus.Waiting,
            CurrentQuestionIndex = -1
        };

        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveAsync();
        return Result<int>.Ok(session.Id);
    }

    public async Task<Result<SessionHostDto>> GetHostViewAsync(int sessionId, int teacherId)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId);
        var check = CheckHost(session, teacherId);
        if (!check.Success)
            return Result<SessionHostDto>.Fail(check.Errors[0], check.ErrorType);

        var dto = new SessionHostDto
        {
            Id = session!.Id,
            QuizId = session.QuizId,
            QuizTitle = session.Quiz.Title,
            JoinCode = session.JoinCode,
            Status = session.Status,
            CurrentQuestionIndex = session.CurrentQuestionIndex,
            QuestionCount = session.Quiz.Questions.Count,
            CurrentQuestion = GetCurrentQuestion(session),
            CreatedAt = session.CreatedAt,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt
        };

        foreach (var p in session.Participants.OrderBy(p => p.JoinedAt))
        {
            dto.Participants.Add(new ParticipantDto
            {
                Id = p.Id,
                Nickname = p.Nickname,
                JoinedAt = p.JoinedAt,
                TotalScore = p.TotalScore
            });
        }

        return Result<SessionHostDto>.Ok(dto);
    }

    // Bekleme salonundan oturumu başlatır, ilk soruya geçer
    public async Task<Result> StartAsync(int sessionId, int teacherId)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId);
        var check = CheckHost(session, teacherId);
        if (!check.Success)
            return check;

        if (session!.Status != SessionStatus.Waiting)
            return Result.Fail("Bu oturum zaten başlamış ya da bitmiş.");

        // Kural: en az bir katılımcı olmadan başlatılamaz
        if (session.Participants.Count == 0)
            return Result.Fail("Oturumu başlatmak için en az bir öğrencinin katılması gerekir.");

        session.Status = SessionStatus.Active;
        session.CurrentQuestionIndex = 0;
        session.StartedAt = DateTime.UtcNow;

        await _sessionRepository.SaveAsync();
        return Result.Ok();
    }

    // Sonraki soruya geçer; son sorudaysa oturumu bitirir
    public async Task<Result> NextQuestionAsync(int sessionId, int teacherId)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId);
        var check = CheckHost(session, teacherId);
        if (!check.Success)
            return check;

        if (session!.Status != SessionStatus.Active)
            return Result.Fail("Sadece devam eden bir oturumda soru değiştirilebilir.");

        int lastIndex = session.Quiz.Questions.Count - 1;
        if (session.CurrentQuestionIndex < lastIndex)
        {
            session.CurrentQuestionIndex++;
        }
        else
        {
            // Son sorudaydık, oturumu bitir
            session.Status = SessionStatus.Finished;
            session.EndedAt = DateTime.UtcNow;
        }

        await _sessionRepository.SaveAsync();
        return Result.Ok();
    }

    public async Task<Result> FinishAsync(int sessionId, int teacherId)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId);
        var check = CheckHost(session, teacherId);
        if (!check.Success)
            return check;

        if (session!.Status != SessionStatus.Active)
            return Result.Fail("Sadece devam eden bir oturum bitirilebilir.");

        session.Status = SessionStatus.Finished;
        session.EndedAt = DateTime.UtcNow;

        await _sessionRepository.SaveAsync();
        return Result.Ok();
    }

    // Henüz başlamamış oturumu iptal eder (siler). Böylece quizin kilidi de açılır.
    public async Task<Result<int>> CancelAsync(int sessionId, int teacherId)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId);
        var check = CheckHost(session, teacherId);
        if (!check.Success)
            return Result<int>.Fail(check.Errors[0], check.ErrorType);

        if (session!.Status != SessionStatus.Waiting)
            return Result<int>.Fail("Başlamış bir oturum iptal edilemez, ancak bitirilebilir.");

        int quizId = session.QuizId;
        _sessionRepository.Remove(session); // katılımcılar da cascade ile silinir
        await _sessionRepository.SaveAsync();
        return Result<int>.Ok(quizId);
    }

    // ==================== ÖĞRENCİ ====================

    public async Task<List<SessionListDto>> GetStudentSessionsAsync(int studentId)
    {
        var sessions = await _sessionRepository.GetByParticipantUserAsync(studentId);

        var list = new List<SessionListDto>();
        foreach (var s in sessions)
        {
            var me = s.Participants.FirstOrDefault(p => p.UserId == studentId);
            list.Add(new SessionListDto
            {
                Id = s.Id,
                QuizTitle = s.Quiz.Title,
                JoinCode = s.JoinCode,
                Status = s.Status,
                ParticipantCount = s.Participants.Count,
                CreatedAt = s.CreatedAt,
                MyNickname = me?.Nickname
            });
        }
        return list;
    }

    // Öğrenciyi katılım koduyla oturuma ekler, oturumun Id'sini döner
    public async Task<Result<int>> JoinAsync(JoinSessionDto dto, int studentId)
    {
        string code = NormalizeJoinCode(dto.JoinCode);
        string nickname = (dto.Nickname ?? "").Trim();

        var errors = new List<string>();
        if (code.Length != SessionRules.JoinCodeLength)
            errors.Add("Katılım kodu 6 karakter olmalıdır.");
        if (nickname.Length < SessionRules.NicknameMinLength || nickname.Length > SessionRules.NicknameMaxLength)
            errors.Add("Takma ad 2-20 karakter olmalıdır.");
        if (errors.Count > 0)
            return Result<int>.Fail(errors);

        var session = await _sessionRepository.GetByJoinCodeAsync(code);
        if (session == null)
            return Result<int>.Fail("Bu koda ait bir oturum bulunamadı. Kodu kontrol edin.");

        // Öğrenci bu oturuma daha önce katıldıysa tekrar ekleme, direkt oturuma götür
        var alreadyJoined = session.Participants.FirstOrDefault(p => p.UserId == studentId);
        if (alreadyJoined != null)
            return Result<int>.Ok(session.Id);

        // Kural: sadece bekleme salonundaki oturuma katılınabilir
        if (session.Status == SessionStatus.Active)
            return Result<int>.Fail("Bu oturum başladı, artık katılamazsınız.");
        if (session.Status == SessionStatus.Finished)
            return Result<int>.Fail("Bu oturum sona erdi.");

        // Kural: bir oturumda en fazla 50 katılımcı
        if (session.Participants.Count >= SessionRules.MaxParticipants)
            return Result<int>.Fail("Oturum dolu, en fazla 50 kişi katılabilir.");

        // Kural: aynı oturumda aynı takma ad iki kez olamaz (büyük/küçük harf fark etmez)
        bool nicknameTaken = session.Participants.Any(p => p.Nickname.ToLower() == nickname.ToLower());
        if (nicknameTaken)
            return Result<int>.Fail("Bu takma ad bu oturumda kullanılıyor, başka bir ad seçin.");

        session.Participants.Add(new Participant
        {
            UserId = studentId, // katılımcıyı giriş yapan öğrenciye bağlıyorum
            Nickname = nickname,
            JoinedAt = DateTime.UtcNow
        });

        await _sessionRepository.SaveAsync();
        return Result<int>.Ok(session.Id);
    }

    public async Task<Result<StudentSessionDto>> GetStudentViewAsync(int sessionId, int studentId)
    {
        var session = await _sessionRepository.GetWithDetailsAsync(sessionId);
        if (session == null)
            return Result<StudentSessionDto>.Fail("Oturum bulunamadı.", ErrorType.NotFound);

        // Sadece bu oturuma katılmış öğrenci görebilir
        var me = session.Participants.FirstOrDefault(p => p.UserId == studentId);
        if (me == null)
            return Result<StudentSessionDto>.Fail("Bu oturuma katılmadınız.", ErrorType.Forbidden);

        var dto = new StudentSessionDto
        {
            SessionId = session.Id,
            QuizTitle = session.Quiz.Title,
            Status = session.Status,
            CurrentQuestionIndex = session.CurrentQuestionIndex,
            QuestionCount = session.Quiz.Questions.Count,
            MyNickname = me.Nickname,
            ParticipantCount = session.Participants.Count,
            CurrentQuestion = GetCurrentQuestion(session) // doğru cevap içermiyor
        };
        return Result<StudentSessionDto>.Ok(dto);
    }

    // ==================== YARDIMCI METOTLAR ====================

    // Oturum var mı ve bu öğretmen mi açmış?
    private Result CheckHost(QuizSession? session, int teacherId)
    {
        if (session == null)
            return Result.Fail("Oturum bulunamadı.", ErrorType.NotFound);
        if (session.HostId != teacherId)
            return Result.Fail("Bu oturum size ait değil.", ErrorType.Forbidden);
        return Result.Ok();
    }

    // Şu an gösterilen soru. Doğru cevap bilgisini bilerek almıyorum.
    private LiveQuestionDto? GetCurrentQuestion(QuizSession session)
    {
        if (session.Status != SessionStatus.Active)
            return null;

        var questions = session.Quiz.Questions.OrderBy(q => q.OrderNo).ToList();
        if (session.CurrentQuestionIndex < 0 || session.CurrentQuestionIndex >= questions.Count)
            return null;

        var question = questions[session.CurrentQuestionIndex];
        var dto = new LiveQuestionDto
        {
            OrderNo = session.CurrentQuestionIndex + 1,
            Text = question.Text,
            TimeLimitSeconds = question.TimeLimitSeconds,
            Points = question.Points
        };
        foreach (var option in question.Options.OrderBy(o => o.OrderNo))
        {
            dto.Options.Add(option.Text); // sadece metin, IsCorrect yok
        }
        return dto;
    }

    // 6 karakterlik, daha önce kullanılmamış bir katılım kodu üretir (ör. K7P2QX)
    private async Task<string> CreateUniqueJoinCodeAsync()
    {
        while (true)
        {
            var chars = new char[SessionRules.JoinCodeLength];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = SessionRules.JoinCodeChars[Random.Shared.Next(SessionRules.JoinCodeChars.Length)];
            }
            string code = new string(chars);

            if (!await _sessionRepository.JoinCodeExistsAsync(code))
                return code;
        }
    }

    // Kodu karşılaştırmadan önce boşlukları silip büyük harfe çeviriyorum ("k7p2qx " -> "K7P2QX")
    private string NormalizeJoinCode(string? code)
    {
        return (code ?? "").Trim().ToUpperInvariant();
    }
}
