namespace FleetControl.Application.DTOs;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    string? Phone);

public record InviteUserDto(
    string FullName,
    string Email,
    string Role,
    string? Phone);
