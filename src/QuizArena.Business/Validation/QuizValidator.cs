using QuizArena.Business.Dtos;

namespace QuizArena.Business.Validation;

// Quiz ve soru kontrolleri. Hata yoksa boş liste döner.
// Formda da kontrol var ama o atlatılabilir, asıl kontrol burada yapılıyor.
public static class QuizValidator
{
    public static List<string> ValidateQuiz(QuizInputDto input)
    {
        var errors = new List<string>();
        string title = (input.Title ?? "").Trim();

        if (title.Length < QuizRules.TitleMinLength || title.Length > QuizRules.TitleMaxLength)
            errors.Add("Quiz başlığı 3-150 karakter olmalıdır.");

        if (input.Description != null && input.Description.Trim().Length > QuizRules.DescriptionMaxLength)
            errors.Add("Açıklama en fazla 500 karakter olabilir.");

        if (input.DefaultTimeLimitSeconds < QuizRules.MinTime || input.DefaultTimeLimitSeconds > QuizRules.MaxTime)
            errors.Add("Varsayılan süre 5-300 saniye arasında olmalıdır.");

        return errors;
    }

    public static List<string> ValidateQuestion(QuestionInputDto input)
    {
        var errors = new List<string>();
        string text = (input.Text ?? "").Trim();

        if (text.Length == 0)
            errors.Add("Soru metni boş olamaz.");
        else if (text.Length > QuizRules.QuestionTextMaxLength)
            errors.Add("Soru metni en fazla 500 karakter olabilir.");

        if (input.TimeLimitSeconds < QuizRules.MinTime || input.TimeLimitSeconds > QuizRules.MaxTime)
            errors.Add("Soru süresi 5-300 saniye arasında olmalıdır.");

        if (input.Points < QuizRules.MinPoints || input.Points > QuizRules.MaxPoints)
            errors.Add("Puan 0-5000 arasında olmalıdır.");

        // Boş bırakılan şık kutularını saymıyoruz
        var filled = GetFilledOptions(input.Options);

        foreach (var option in input.Options)
        {
            if (string.IsNullOrWhiteSpace(option.Text) && option.IsCorrect)
            {
                errors.Add("Boş bir şık doğru cevap olarak işaretlenemez.");
                break;
            }
        }

        if (filled.Count < QuizRules.MinOptions)
            errors.Add("En az 2 şık girilmelidir.");
        if (filled.Count > QuizRules.MaxOptions)
            errors.Add("En fazla 6 şık girilebilir.");

        int correctCount = filled.Count(o => o.IsCorrect);
        if (correctCount == 0)
            errors.Add("Bir doğru cevap seçilmelidir.");
        if (correctCount > 1)
            errors.Add("Sadece bir şık doğru olabilir.");

        if (filled.Any(o => o.Text!.Length > QuizRules.OptionTextMaxLength))
            errors.Add("Şık metni en fazla 200 karakter olabilir.");

        // Aynı şık iki kere yazılmış mı? (büyük/küçük harf fark etmez)
        var texts = filled.Select(o => o.Text!.ToLower()).ToList();
        if (texts.Count != texts.Distinct().Count())
            errors.Add("Aynı şık birden fazla kez girilemez.");

        return errors;
    }

    // Boş şıkları atar, dolu olanların başındaki/sonundaki boşlukları siler
    public static List<OptionInputDto> GetFilledOptions(List<OptionInputDto> options)
    {
        var result = new List<OptionInputDto>();
        foreach (var option in options)
        {
            if (!string.IsNullOrWhiteSpace(option.Text))
            {
                result.Add(new OptionInputDto { Text = option.Text.Trim(), IsCorrect = option.IsCorrect });
            }
        }
        return result;
    }
}
