using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Authentication.Dtos;

public record LoginResponseDto(string Token, UserResponseDto User);