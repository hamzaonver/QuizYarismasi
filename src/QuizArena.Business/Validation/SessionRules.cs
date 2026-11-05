namespace QuizArena.Business.Validation;

// Canlı oturum kuralları için sabit değerler (Hafta 7)
public static class SessionRules
{
    public const int JoinCodeLength = 6;

    // Karışmasın diye 0/O ve 1/I harflerini koymadım
    public const string JoinCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public const int NicknameMinLength = 2;
    public const int NicknameMaxLength = 20;

    public const int MaxParticipants = 50;
}
