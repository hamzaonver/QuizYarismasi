namespace QuizArena.Entities.Enums;

// Canlı oturumun durumu (Hafta 7'de kullanılacak)
public enum SessionStatus
{
    Waiting = 0,  // Öğrenciler katılıyor
    Active = 1,   // Sorular soruluyor
    Finished = 2  // Bitti
}
