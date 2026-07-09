namespace FleetControl.Application.Common.Interfaces;

/// <summary>
/// Cliente hacia la API de administracion de Supabase Auth (usa el
/// service_role key). Se usa SOLO para invitar usuarios nuevos: crea el
/// registro en auth.users y Supabase envia un correo con un enlace magico
/// para que el usuario establezca su contrasena por primera vez.
/// </summary>
public interface ISupabaseAuthAdminService
{
    /// <summary>Crea el usuario en Supabase Auth y envia el correo de invitacion. Devuelve su Id (Guid).</summary>
    Task<Guid> InviteUserByEmailAsync(string email, CancellationToken ct = default);
}
