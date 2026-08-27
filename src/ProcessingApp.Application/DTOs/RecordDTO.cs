namespace ProcessingApp.Application.DTOs;

public readonly record struct RecordDTO(
    DateTime Date,
    string FirstName,
    string LastName,
    string SurName,
    string City,
    string Country
);