using FleetControl.Application.DTOs;

namespace FleetControl.Application.Services;

public interface IUserService
{
    Task<UserDto> GetCurrentUserAsync(CancellationToken ct = default);

    /// <summary>Lista los usuarios del tenant actual (solo Admin).</summary>
    Task<IReadOnlyList<UserDto>> GetTenantUsersAsync(CancellationToken ct = default);

    /// <summary>Invita a un usuario nuevo por correo (solo Admin): crea el registro en Supabase Auth y el perfil de negocio.</summary>
    Task<UserDto> InviteUserAsync(InviteUserDto dto, CancellationToken ct = default);
}
