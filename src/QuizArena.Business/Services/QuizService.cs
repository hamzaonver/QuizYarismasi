using QuizArena.Business.Common;
using QuizArena.Business.Dtos;
using QuizArena.Business.Validation;
using QuizArena.Data.Repositories;
using QuizArena.Entities;

namespace QuizArena.Business.Services;

// Quiz Oluşturma modülü (Hafta 6)
// Her metotta teacherId alıyorum ve quizin o öğretmene ait olup olmadığını kontrol ediyorum.
// (Sadece butonu gizlemek yetmez, biri adres çubuğundan başka quizin id'sini yazabilir.)
public class QuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;

    public QuizService(IQuizRepository quizRepository, IQuestionRepository questionRepository)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
    }

    // ==================== QUIZ ====================

    public async Task<List<QuizListDto>> GetTeacherQuizzesAsync(int teacherId)
    {
        var quizzes = await _quizRepository.GetByTeacherAsync(teacherId);

        var list = new List<QuizListDto>();
        foreach (var q in quizzes)
        {
            list.Add(new QuizListDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                IsPublished = q.IsPublished,
                QuestionCount = q.Questions.Count,
                SessionCount = q.Sessions.Count,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            });
        }
        return list;
    }

    public async Task<Result<QuizDetailDto>> GetDetailAsync(int quizId, int teacherId)
    {
        var quiz = await _quizRepository.GetWithQuestionsAsync(quizId);

        var check = CheckOwner(quiz, teacherId);
        if (!check.Success)
            return Result<QuizDetailDto>.Fail(check.Errors[0], check.ErrorType);

        var dto = new QuizDetailDto
        {
            Id = quiz!.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            DefaultTimeLimitSeconds = quiz.DefaultTimeLimitSeconds,
            IsPublished = quiz.IsPublished,
            IsLocked = await _quizRepository.IsUsedInSessionAsync(quizId)
        };
        foreach (var question in quiz.Questions)
        {
            dto.Questions.Add(ToQuestionDto(question));
        }

        return Result<QuizDetailDto>.Ok(dto);
    }

    public async Task<Result<int>> CreateAsync(int teacherId, QuizInputDto input)
    {
        var errors = QuizValidator.ValidateQuiz(input);
        if (errors.Count > 0)
            return Result<int>.Fail(errors);

        var quiz = new Quiz
        {
            TeacherId = teacherId, // formdan değil, giriş yapan kullanıcıdan
            Title = input.Title.Trim(),
            Description = CleanText(input.Description),
            DefaultTimeLimitSeconds = input.DefaultTimeLimitSeconds,
            IsPublished = false // yeni quiz taslak olarak başlar
        };

        await _quizRepository.AddAsync(quiz);
        await _quizRepository.SaveAsync();

        return Result<int>.Ok(quiz.Id);
    }

    public async Task<Result> UpdateAsync(int quizId, int teacherId, QuizInputDto input)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);

        var check = CheckOwner(quiz, teacherId);
        if (!check.Success)
            return check;

        var errors = QuizValidator.ValidateQuiz(input);
        if (errors.Count > 0)
            return Result.Fail(errors);

        // Başlık ve açıklama değişince eski sonuçlar bozulmaz, kilitli quizde de izin veriyorum
        quiz!.Title = input.Title.Trim();
        quiz.Description = CleanText(input.Description);
        quiz.DefaultTimeLimitSeconds = input.DefaultTimeLimitSeconds;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizRepository.SaveAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int quizId, int teacherId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);

        var check = CheckOwner(quiz, teacherId);
        if (!check.Success)
            return check;

        // Oturumu olan quiz silinirse eski sonuçlar kaybolur
        if (await _quizRepository.IsUsedInSessionAsync(quizId))
            return Result.Fail("Bu quiz oturumlarda kullanıldığı için silinemez. İsterseniz yayından kaldırabilirsiniz.");

        _quizRepository.Remove(quiz!); // sorular ve şıklar da cascade ile silinir
        await _quizRepository.SaveAsync();
        return Result.Ok();
    }

    public async Task<Result> SetPublishedAsync(int quizId, int teacherId, bool publish)
    {
        var quiz = await _quizRepository.GetWithQuestionsAsync(quizId);

        var check = CheckOwner(quiz, teacherId);
        if (!check.Success)
            return check;

        if (publish && quiz!.Questions.Count == 0)
            return Result.Fail("Yayınlamak için quizde en az bir soru olmalıdır.");

        // Hafta 8 düzeltmesi: açık oturumu olan quiz yayından kaldırılırsa öğrenciler
        // bekleme salonunda taslak bir quizle kalıyordu. Önce oturum bitirilmeli.
        if (!publish && await _quizRepository.HasOpenSessionAsync(quizId))
            return Result.Fail("Bu quizin açık bir oturumu var. Önce oturumu bitirin ya da iptal edin.");

        quiz!.IsPublished = publish;
        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.SaveAsync();
        return Result.Ok();
    }

    // Quizin bir kopyasını oluşturur (kilitli quizi değiştirmek isteyen öğretmen için)
    public async Task<Result<int>> DuplicateAsync(int quizId, int teacherId)
    {
        var source = await _quizRepository.GetWithQuestionsAsync(quizId);

        var check = CheckOwner(source, teacherId);
        if (!check.Success)
            return Result<int>.Fail(check.Errors[0], check.ErrorType);

        string title = source!.Title + " (Kopya)";
        if (title.Length > QuizRules.TitleMaxLength)
            title = title.Substring(0, QuizRules.TitleMaxLength);

        var copy = new Quiz
        {
            TeacherId = teacherId,
            Title = title,
            Description = source.Description,
            DefaultTimeLimitSeconds = source.DefaultTimeLimitSeconds,
            IsPublished = false
        };

        foreach (var q in source.Questions)
        {
            var newQuestion = new Question
            {
                OrderNo = q.OrderNo,
                Text = q.Text,
                TimeLimitSeconds = q.TimeLimitSeconds,
                Points = q.Points
            };
            foreach (var o in q.Options)
            {
                newQuestion.Options.Add(new AnswerOption { OrderNo = o.OrderNo, Text = o.Text, IsCorrect = o.IsCorrect });
            }
            copy.Questions.Add(newQuestion);
        }

        await _quizRepository.AddAsync(copy);
        await _quizRepository.SaveAsync();
        return Result<int>.Ok(copy.Id);
    }

    // ==================== SORU ====================

    public async Task<Result<QuestionDto>> GetQuestionAsync(int questionId, int teacherId)
    {
        var question = await _questionRepository.GetWithOptionsAsync(questionId);
        if (question == null)
            return Result<QuestionDto>.Fail("Soru bulunamadı.", ErrorType.NotFound);

        var check = CheckOwner(question.Quiz, teacherId);
        if (!check.Success)
            return Result<QuestionDto>.Fail(check.Errors[0], check.ErrorType);

        return Result<QuestionDto>.Ok(ToQuestionDto(question));
    }

    public async Task<Result<int>> AddQuestionAsync(int quizId, int teacherId, QuestionInputDto input)
    {
        var quiz = await _quizRepository.GetWithQuestionsAsync(quizId);

        var check = CheckOwner(quiz, teacherId);
        if (!check.Success)
            return Result<int>.Fail(check.Errors[0], check.ErrorType);

        if (await _quizRepository.IsUsedInSessionAsync(quizId))
            return Result<int>.Fail(QuizRules.LockedMessage);

        if (quiz!.Questions.Count >= QuizRules.MaxQuestions)
            return Result<int>.Fail("Bir quizde en fazla 50 soru olabilir.");

        var errors = QuizValidator.ValidateQuestion(input);
        if (errors.Count > 0)
            return Result<int>.Fail(errors);

        // Yeni soru en sona eklenir
        int nextOrder = 1;
        if (quiz.Questions.Count > 0)
            nextOrder = quiz.Questions.Max(q => q.OrderNo) + 1;

        var question = new Question
        {
            QuizId = quizId,
            OrderNo = nextOrder,
            Text = input.Text.Trim(),
            TimeLimitSeconds = input.TimeLimitSeconds,
            Points = input.Points,
            Options = CreateOptions(input.Options)
        };

        await _questionRepository.AddAsync(question);
        quiz.UpdatedAt = DateTime.UtcNow;
        await _questionRepository.SaveAsync();

        return Result<int>.Ok(question.Id);
    }

    public async Task<Result> UpdateQuestionAsync(int questionId, int teacherId, QuestionInputDto input)
    {
        var question = await _questionRepository.GetWithOptionsAsync(questionId);
        if (question == null)
            return Result.Fail("Soru bulunamadı.", ErrorType.NotFound);

        var check = CheckOwner(question.Quiz, teacherId);
        if (!check.Success)
            return check;

        if (await _quizRepository.IsUsedInSessionAsync(question.QuizId))
            return Result.Fail(QuizRules.LockedMessage);

        var errors = QuizValidator.ValidateQuestion(input);
        if (errors.Count > 0)
            return Result.Fail(errors);

        question.Text = input.Text.Trim();
        question.TimeLimitSeconds = input.TimeLimitSeconds;
        question.Points = input.Points;

        // Eski şıkları silip yenilerini ekliyorum (en basit yöntem)
        _questionRepository.RemoveOptions(question.Options.ToList());
        question.Options = CreateOptions(input.Options);

        question.Quiz.UpdatedAt = DateTime.UtcNow;
        await _questionRepository.SaveAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteQuestionAsync(int questionId, int teacherId)
    {
        var question = await _questionRepository.GetWithOptionsAsync(questionId);
        if (question == null)
            return Result.Fail("Soru bulunamadı.", ErrorType.NotFound);

        var check = CheckOwner(question.Quiz, teacherId);
        if (!check.Success)
            return check;

        if (await _quizRepository.IsUsedInSessionAsync(question.QuizId))
            return Result.Fail(QuizRules.LockedMessage);

        var quiz = question.Quiz;
        _questionRepository.Remove(question);

        // Kalan soruları tekrar 1, 2, 3 diye numarala
        var allQuestions = await _questionRepository.GetByQuizAsync(quiz.Id);
        var remaining = allQuestions.Where(q => q.Id != questionId).ToList();
        for (int i = 0; i < remaining.Count; i++)
        {
            remaining[i].OrderNo = i + 1;
        }

        // Hiç soru kalmadıysa quiz yayında kalamaz
        if (remaining.Count == 0)
            quiz.IsPublished = false;

        quiz.UpdatedAt = DateTime.UtcNow;
        await _questionRepository.SaveAsync();
        return Result.Ok();
    }

    // direction: -1 = yukarı, +1 = aşağı
    public async Task<Result> MoveQuestionAsync(int questionId, int teacherId, int direction)
    {
        var question = await _questionRepository.GetWithOptionsAsync(questionId);
        if (question == null)
            return Result.Fail("Soru bulunamadı.", ErrorType.NotFound);

        var check = CheckOwner(question.Quiz, teacherId);
        if (!check.Success)
            return check;

        if (await _quizRepository.IsUsedInSessionAsync(question.QuizId))
            return Result.Fail(QuizRules.LockedMessage);

        var questions = await _questionRepository.GetByQuizAsync(question.QuizId);
        int index = questions.FindIndex(q => q.Id == questionId);
        int otherIndex = index + direction;

        // En üstteki yukarı, en alttaki aşağı gidemez
        if (otherIndex < 0 || otherIndex >= questions.Count)
            return Result.Ok();

        // İki sorunun sıra numarasını değiştir
        int temp = questions[index].OrderNo;
        questions[index].OrderNo = questions[otherIndex].OrderNo;
        questions[otherIndex].OrderNo = temp;

        await _questionRepository.SaveAsync();
        return Result.Ok();
    }

    // ==================== YARDIMCI METOTLAR ====================

    // Quiz var mı ve bu öğretmene mi ait?
    private Result CheckOwner(Quiz? quiz, int teacherId)
    {
        if (quiz == null)
            return Result.Fail("Quiz bulunamadı.", ErrorType.NotFound);

        if (quiz.TeacherId != teacherId)
            return Result.Fail("Bu quiz size ait değil.", ErrorType.Forbidden);

        return Result.Ok();
    }

    private List<AnswerOption> CreateOptions(List<OptionInputDto> options)
    {
        var filled = QuizValidator.GetFilledOptions(options);
        var result = new List<AnswerOption>();
        for (int i = 0; i < filled.Count; i++)
        {
            result.Add(new AnswerOption
            {
                Text = filled[i].Text!,
                OrderNo = i + 1,
                IsCorrect = filled[i].IsCorrect
            });
        }
        return result;
    }

    private QuestionDto ToQuestionDto(Question question)
    {
        var dto = new QuestionDto
        {
            Id = question.Id,
            QuizId = question.QuizId,
            OrderNo = question.OrderNo,
            Text = question.Text,
            TimeLimitSeconds = question.TimeLimitSeconds,
            Points = question.Points
        };
        foreach (var o in question.Options.OrderBy(x => x.OrderNo))
        {
            dto.Options.Add(new OptionDto { Id = o.Id, OrderNo = o.OrderNo, Text = o.Text, IsCorrect = o.IsCorrect });
        }
        return dto;
    }

    private string? CleanText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        return text.Trim();
    }
}
