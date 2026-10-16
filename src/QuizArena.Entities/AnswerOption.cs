namespace QuizArena.Entities;

// Sorunun bir şıkkı. Doğru şıkta IsCorrect = true olur.
public class AnswerOption : BaseEntity
{
    public int QuestionId { get; set; }         // FK -> Questions
    public Question Question { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public int OrderNo { get; set; }            // A=1, B=2 ...
    public bool IsCorrect { get; set; }
}
