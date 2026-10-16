namespace QuizArena.Business.Validation;

// Quiz kuralları için sabit değerler. Hepsini tek yerde tuttum ki değiştirmesi kolay olsun.
public static class QuizRules
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 150;
    public const int DescriptionMaxLength = 500;
    public const int QuestionTextMaxLength = 500;
    public const int OptionTextMaxLength = 200;

    public const int MinOptions = 2;
    public const int MaxOptions = 6;

    public const int MinTime = 5;     // saniye
    public const int MaxTime = 300;

    public const int MinPoints = 0;
    public const int MaxPoints = 5000;

    public const int MaxQuestions = 50;

    public const string LockedMessage = "Bu quiz bir oturumda kullanıldığı için soruları değiştirilemez. " +
                                        "Değişiklik yapmak için quizi kopyalayın.";
}
