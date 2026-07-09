using System.Net;
using System.Text;
using System.Text.Json;
using FleetControl.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FleetControl.Application.UnitTests.Infrastructure;

/// <summary>
/// Pruebas de SupabaseAuthAdminService contra un HttpMessageHandler falso
/// (sin red real), verificando tanto el camino feliz (Supabase devuelve el
/// Guid del usuario creado) como el de error (Supabase responde con un
/// status code de fallo).
/// </summary>
public class SupabaseAuthAdminServiceTests
{
    private static SupabaseAuthAdminService CreateSut(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new SupabaseSettings { Url = "https://project.supabase.co", ServiceRoleKey = "service-role-key" });
        return new SupabaseAuthAdminService(httpClient, settings);
    }

    [Fact]
    public async Task InviteUserByEmailAsync_DebeRetornarElIdDelUsuario_CuandoSupabaseRespondeExitosamente()
    {
        var expectedId = Guid.NewGuid();
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { id = expectedId }), Encoding.UTF8, "application/json")
            });

        var sut = CreateSut(handler);

        var result = await sut.InviteUserByEmailAsync("nuevo@test.com");

        result.Should().Be(expectedId);
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/auth/v1/invite");
        handler.LastRequest.Headers.Authorization!.Parameter.Should().Be("service-role-key");
    }

    [Fact]
    public async Task InviteUserByEmailAsync_DebeLanzarExcepcion_CuandoSupabaseRespondeConError()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"email ya registrado\"}", Encoding.UTF8, "application/json")
            });

        var sut = CreateSut(handler);

        var act = async () => await sut.InviteUserByEmailAsync("duplicado@test.com");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InviteUserByEmailAsync_DebeLanzarExcepcion_CuandoLaRespuestaVieneVacia()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });

        var sut = CreateSut(handler);

        var act = async () => await sut.InviteUserByEmailAsync("nuevo@test.com");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public HttpRequestMessage? LastRequest { get; private set; }

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responder(request));
        }
    }
}
