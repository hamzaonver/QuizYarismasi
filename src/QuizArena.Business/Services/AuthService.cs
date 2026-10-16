using QuizArena.Business.Common;
using QuizArena.Business.Dtos;
using QuizArena.Business.Helpers;
using QuizArena.Business.Validation;
using QuizArena.Data.Repositories;
using QuizArena.Entities;
using QuizArena.Entities.Enums;

namespace QuizArena.Business.Services;

// Kayıt ve giriş işlemleri
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly AuthSettings _settings;

    public AuthService(IUserRepository userRepository, AuthSettings settings)
    {
        _userRepository = userRepository;
        _settings = settings;
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterDto dto)
    {
        var errors = UserValidator.ValidateRegister(dto);

        // Herkes kendini öğretmen yapamasın diye davet kodu istiyorum
        if (dto.Role == UserRole.Teacher)
        {
            if (string.IsNullOrEmpty(_settings.TeacherInviteCode))
                errors.Add("Öğretmen kaydı şu an kapalıdır.");
            else if (dto.TeacherInviteCode?.Trim() != _settings.TeacherInviteCode)
                errors.Add("Öğretmen davet kodu geçersiz.");
        }

        if (errors.Count > 0)
            return Result<UserDto>.Fail(errors);

        string email = UserValidator.NormalizeEmail(dto.Email);
        if (await _userRepository.EmailExistsAsync(email))
            return Result<UserDto>.Fail("Bu e-posta adresiyle zaten bir hesap var.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(dto.Password), // düz parola kaydedilmiyor
            Role = dto.Role
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveAsync();

        return Result<UserDto>.Ok(ToDto(user));
    }

    public async Task<Result<UserDto>> LoginAsync(LoginDto dto)
    {
        // Hangisinin yanlış olduğunu söylemiyorum, böylece kayıtlı e-postalar öğrenilemez
        const string wrongMessage = "E-posta veya parola hatalı.";

        string email = UserValidator.NormalizeEmail(dto.Email);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return Result<UserDto>.Fail(wrongMessage);

        // Hesap kilitli mi?
        if (user.LockoutEndUtc != null && user.LockoutEndUtc > DateTime.UtcNow)
        {
            int minutes = (int)Math.Ceiling((user.LockoutEndUtc.Value - DateTime.UtcNow).TotalMinutes);
            return Result<UserDto>.Fail($"Çok fazla hatalı deneme yapıldı. Hesap {minutes} dakika sonra açılacak.");
        }

        if (!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
        {
            // Hatalı deneme sayısını artır, sınıra gelince kilitle
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= _settings.MaxFailedLoginAttempts)
            {
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(_settings.LockoutMinutes);
                user.FailedLoginCount = 0;
            }
            await _userRepository.SaveAsync();
            return Result<UserDto>.Fail(wrongMessage);
        }

        // Giriş başarılı, sayacı sıfırla
        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.SaveAsync();

        return Result<UserDto>.Ok(ToDto(user));
    }

    private UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}
