namespace QuizArena.Business;

// appsettings.json içindeki "Auth" bölümünden okunuyor
public class AuthSettings
{
    // Öğretmen olarak kayıt olmak için gereken kod (boşsa öğretmen kaydı kapalı)
    public string TeacherInviteCode { get; set; } = string.Empty;

    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 5;
}
