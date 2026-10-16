namespace QuizArena.Entities;

// Bütün tablolarda ortak olan alanlar
public abstract class BaseEntity
{
    public int Id { get; set; } // Birincil anahtar (PK)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
