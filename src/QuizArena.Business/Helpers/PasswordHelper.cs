namespace QuizArena.Business.Helpers;

// Parola hash işlemleri. BCrypt her parolaya otomatik rastgele bir tuz (salt) ekler
// ve tuzu hash'in içinde saklar. Bu yüzden aynı parolanın hash'i her seferinde farklı çıkar.
public static class PasswordHelper
{
    // 11 = maliyet. Büyüdükçe hash'leme yavaşlar, deneme-yanılma saldırısı zorlaşır.
    private const int WorkFactor = 11;

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false; // hash bozuksa giriş başarısız
        }
    }
}
