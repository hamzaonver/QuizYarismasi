namespace QuizArena.Business.Dtos;

// ===== Formdan gelen veriler =====

public class QuizInputDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultTimeLimitSeconds { get; set; }
}

public class OptionInputDto
{
    public string? Text { get; set; }
    public bool IsCorrect { get; set; }
}

public class QuestionInputDto
{
    public string Text { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public int Points { get; set; }
    public List<OptionInputDto> Options { get; set; } = new();
}

// ===== Sayfalara giden veriler =====

public class QuizListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public int QuestionCount { get; set; }
    public int SessionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OptionDto
{
    public int Id { get; set; }
    public int OrderNo { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public int OrderNo { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public int Points { get; set; }
    public List<OptionDto> Options { get; set; } = new();
}

public class QuizDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultTimeLimitSeconds { get; set; }
    public bool IsPublished { get; set; }
    public bool IsLocked { get; set; } // oturumda kullanıldıysa sorular kilitli
    public List<QuestionDto> Questions { get; set; } = new();
}
