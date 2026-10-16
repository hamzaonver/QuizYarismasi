namespace QuizArena.Business.Common;

public enum ErrorType
{
    None,
    Validation, // Kurala uymayan giriş -> kullanıcıya mesaj göster
    NotFound,   // Kayıt yok -> 404
    Forbidden   // Kayıt başkasına ait -> 403
}

// Servislerden dönen sonuç: işlem başarılı mı, değilse hata mesajları ne?
public class Result
{
    public bool Success { get; set; }
    public ErrorType ErrorType { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result Ok()
    {
        return new Result { Success = true };
    }

    public static Result Fail(string message, ErrorType type = ErrorType.Validation)
    {
        return new Result { Success = false, ErrorType = type, Errors = new List<string> { message } };
    }

    public static Result Fail(List<string> errors)
    {
        return new Result { Success = false, ErrorType = ErrorType.Validation, Errors = errors };
    }
}

// Başarılı olunca bir de veri döndüren hali (örneğin yeni oluşan quizin Id'si)
public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Ok(T data)
    {
        return new Result<T> { Success = true, Data = data };
    }

    public static new Result<T> Fail(string message, ErrorType type = ErrorType.Validation)
    {
        return new Result<T> { Success = false, ErrorType = type, Errors = new List<string> { message } };
    }

    public static new Result<T> Fail(List<string> errors)
    {
        return new Result<T> { Success = false, ErrorType = ErrorType.Validation, Errors = errors };
    }
}
