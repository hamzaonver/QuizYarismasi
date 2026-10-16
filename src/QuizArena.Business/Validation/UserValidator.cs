using System.Text.RegularExpressions;
using QuizArena.Business.Dtos;

namespace QuizArena.Business.Validation;

public static class UserValidator
{
    public static List<string> ValidateRegister(RegisterDto dto)
    {
        var errors = new List<string>();

        string name = (dto.FullName ?? "").Trim();
        if (name.Length < 2 || name.Length > 100)
            errors.Add("Ad soyad 2-100 karakter olmalıdır.");

        // Basit e-posta kontrolü: içinde @ ve nokta olmalı, boşluk olmamalı
        string email = NormalizeEmail(dto.Email);
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Geçerli bir e-posta adresi giriniz.");

        errors.AddRange(ValidatePassword(dto.Password));
        return errors;
    }

    // Parola kuralı: en az 8 karakter, büyük harf, küçük harf ve rakam
    public static List<string> ValidatePassword(string? password)
    {
        var errors = new List<string>();
        password = password ?? "";

        if (password.Length < 8)
            errors.Add("Parola en az 8 karakter olmalıdır.");
        if (!password.Any(char.IsUpper))
            errors.Add("Parola en az bir büyük harf içermelidir.");
        if (!password.Any(char.IsLower))
            errors.Add("Parola en az bir küçük harf içermelidir.");
        if (!password.Any(char.IsDigit))
            errors.Add("Parola en az bir rakam içermelidir.");

        return errors;
    }

    // E-postayı karşılaştırmadan önce küçük harfe çevirip boşlukları siliyorum
    public static string NormalizeEmail(string? email)
    {
        return (email ?? "").Trim().ToLowerInvariant(); // Invariant: Türkçe "I" -> "ı" sorunu olmasın
    }
}
