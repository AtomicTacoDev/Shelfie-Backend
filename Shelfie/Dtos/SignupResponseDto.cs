namespace Shelfie.Models.Dto;

public record SignupResponseDto(bool Success, string? Message, string? ConfirmationToken, string? Username);