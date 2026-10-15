namespace ScholarshipPlatform.Users.Dtos;

public record CreateUserDto(
    string Name,
    string Email,
    string Password,
    DateOnly BirthDate,
    string PhoneNumber   
);