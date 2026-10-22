namespace ProcessingApp.Domain;

/// <summary>
/// Сущность БД. Намеренно оставлена классом, а не record: EF Core отслеживает сущности
/// по ссылочному равенству, а record подменяет его значимым (Equals/GetHashCode по всем полям).
/// Структуры сущностями быть не могут вовсе. Для передачи данных между слоями
/// используется <c>RecordDTO</c> (readonly record struct).
/// </summary>
public sealed class Record
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string SurName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}
