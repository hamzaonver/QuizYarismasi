using System.ComponentModel.DataAnnotations;
using QuizArena.Business.Dtos;

namespace QuizArena.Web.Models;

public class QuizFormViewModel
{
    public int? Id { get; set; } // null ise yeni quiz

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Başlık 3-150 karakter olmalıdır.")]
    [Display(Name = "Quiz başlığı")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Range(5, 300, ErrorMessage = "Süre 5-300 saniye arasında olmalıdır.")]
    [Display(Name = "Varsayılan soru süresi (sn)")]
    public int DefaultTimeLimitSeconds { get; set; } = 20;

    public QuizInputDto ToDto()
    {
        return new QuizInputDto
        {
            Title = Title,
            Description = Description,
            DefaultTimeLimitSeconds = DefaultTimeLimitSeconds
        };
    }
}

public class QuestionFormViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int? QuestionId { get; set; } // null ise yeni soru
    public int? OrderNo { get; set; }

    [Required(ErrorMessage = "Soru metni zorunludur.")]
    [StringLength(500, ErrorMessage = "Soru metni en fazla 500 karakter olabilir.")]
    [Display(Name = "Soru")]
    public string Text { get; set; } = string.Empty;

    [Range(5, 300, ErrorMessage = "Süre 5-300 saniye arasında olmalıdır.")]
    [Display(Name = "Süre (sn)")]
    public int TimeLimitSeconds { get; set; } = 20;

    [Range(0, 5000, ErrorMessage = "Puan 0-5000 arasında olmalıdır.")]
    [Display(Name = "Puan")]
    public int Points { get; set; } = 1000;

    // Şık metinleri (formda 6 kutu var, boş bırakılanlar sayılmaz)
    public List<string?> Options { get; set; } = new();

    // Doğru şıkkın sırası (0 = A, 1 = B ...)
    public int? CorrectIndex { get; set; }

    // Formda her zaman 6 şık kutusu olsun
    public void FillEmptyOptions()
    {
        while (Options.Count < 6)
            Options.Add(null);
    }

    public QuestionInputDto ToDto()
    {
        var dto = new QuestionInputDto
        {
            Text = Text,
            TimeLimitSeconds = TimeLimitSeconds,
            Points = Points
        };
        for (int i = 0; i < Options.Count; i++)
        {
            dto.Options.Add(new OptionInputDto { Text = Options[i], IsCorrect = (i == CorrectIndex) });
        }
        return dto;
    }

    public static QuestionFormViewModel FromDto(QuestionDto question, string quizTitle)
    {
        var model = new QuestionFormViewModel
        {
            QuizId = question.QuizId,
            QuizTitle = quizTitle,
            QuestionId = question.Id,
            OrderNo = question.OrderNo,
            Text = question.Text,
            TimeLimitSeconds = question.TimeLimitSeconds,
            Points = question.Points
        };

        for (int i = 0; i < question.Options.Count; i++)
        {
            model.Options.Add(question.Options[i].Text);
            if (question.Options[i].IsCorrect)
                model.CorrectIndex = i;
        }

        model.FillEmptyOptions();
        return model;
    }
}
