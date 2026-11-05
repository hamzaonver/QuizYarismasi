using System.ComponentModel.DataAnnotations;
using QuizArena.Business.Dtos;

namespace QuizArena.Web.Models;

// Öğrencinin katılım formu
public class JoinSessionViewModel
{
    [Required(ErrorMessage = "Katılım kodu zorunludur.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Katılım kodu 6 karakter olmalıdır.")]
    [Display(Name = "Katılım kodu")]
    public string JoinCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Takma ad zorunludur.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Takma ad 2-20 karakter olmalıdır.")]
    [Display(Name = "Takma ad")]
    public string Nickname { get; set; } = string.Empty;

    public JoinSessionDto ToDto()
    {
        return new JoinSessionDto { JoinCode = JoinCode, Nickname = Nickname };
    }
}

// Öğrenci paneli: katılım formu + katıldığı oturumlar
public class StudentPanelViewModel
{
    public JoinSessionViewModel Join { get; set; } = new();
    public List<SessionListDto> MySessions { get; set; } = new();
}
