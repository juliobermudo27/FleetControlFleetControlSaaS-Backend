using FleetControl.Application.DTOs;
using FleetControl.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetControl.WebAPI.Controllers;

/// <summary>Perfil del usuario actual y gestion de usuarios del tenant (invitaciones, solo Admin).</summary>
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken ct)
        => Ok(await _userService.GetCurrentUserAsync(ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken ct)
        => Ok(await _userService.GetTenantUsersAsync(ct));

    [HttpPost("invite")]
    public async Task<ActionResult<UserDto>> Invite([FromBody] InviteUserDto dto, CancellationToken ct)
        => Ok(await _userService.InviteUserAsync(dto, ct));
}
