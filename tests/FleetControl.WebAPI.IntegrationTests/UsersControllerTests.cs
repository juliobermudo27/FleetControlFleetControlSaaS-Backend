using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FleetControl.WebAPI.IntegrationTests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateAuthenticatedClient(Guid userId)
    {
        var client = _factory.CreateClient();
        var token = TestJwtGenerator.GenerateToken(userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetMe_SinToken_DebeRetornar401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ComoAdmin_DebeRetornarSuPropioPerfil()
    {
        var client = CreateAuthenticatedClient(_factory.AdminTenantAId);

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        body!.Id.Should().Be(_factory.AdminTenantAId);
        body.Role.Should().Be("admin");
    }

    [Fact]
    public async Task GetAll_ComoConductor_DebeRetornar403()
    {
        var client = CreateAuthenticatedClient(_factory.DriverTenantAId);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_ComoAdmin_DebeRetornar200_YSoloUsuariosDeSuTenant()
    {
        var client = CreateAuthenticatedClient(_factory.AdminTenantAId);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(u => u.Id == _factory.AdminTenantAId);
        body.Should().NotContain(u => u.Id == _factory.AdminTenantBId);
    }

    [Fact]
    public async Task Invite_SinToken_DebeRetornar401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/invite", new { fullName = "X", email = "x@test.com", role = "driver" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Invite_ComoConductor_DebeRetornar403()
    {
        var client = CreateAuthenticatedClient(_factory.DriverTenantAId);

        var response = await client.PostAsJsonAsync("/api/users/invite", new { fullName = "X", email = "nuevo1@test.com", role = "driver" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invite_ConRolInvalido_DebeRetornar400()
    {
        var client = CreateAuthenticatedClient(_factory.AdminTenantAId);

        var response = await client.PostAsJsonAsync("/api/users/invite", new { fullName = "X", email = "nuevo2@test.com", role = "superadmin" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invite_ComoAdmin_ConDatosValidos_DebeRetornar200YCrearElUsuario()
    {
        var client = CreateAuthenticatedClient(_factory.AdminTenantAId);

        var response = await client.PostAsJsonAsync("/api/users/invite", new
        {
            fullName = "Conductor Invitado",
            email = "invitado@test.com",
            role = "driver"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        body!.FullName.Should().Be("Conductor Invitado");
        body.Role.Should().Be("driver");
        body.IsActive.Should().BeTrue();

        // El usuario invitado ya deberia poder ver su propio perfil.
        var newUserClient = CreateAuthenticatedClient(body.Id);
        var meResponse = await newUserClient.GetAsync("/api/users/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Invite_ConCorreoYaExistenteEnElTenant_DebeRetornar400()
    {
        var client = CreateAuthenticatedClient(_factory.AdminTenantAId);
        await client.PostAsJsonAsync("/api/users/invite", new { fullName = "Uno", email = "repetido@test.com", role = "driver" });

        var response = await client.PostAsJsonAsync("/api/users/invite", new { fullName = "Dos", email = "repetido@test.com", role = "driver" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record UserResponse(Guid Id, string FullName, string Email, string Role, bool IsActive);
}
