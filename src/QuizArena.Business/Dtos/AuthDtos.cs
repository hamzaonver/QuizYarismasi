using QuizArena.Entities.Enums;

namespace QuizArena.Business.Dtos;

// DTO: katmanlar arasında veri taşımak için kullandığım basit sınıflar.
// Entity'yi direkt arayüze vermiyorum (mesela PasswordHash arayüze hiç gitmesin diye).

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? TeacherInviteCode { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Giriş başarılı olunca çereze yazılacak bilgiler
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
