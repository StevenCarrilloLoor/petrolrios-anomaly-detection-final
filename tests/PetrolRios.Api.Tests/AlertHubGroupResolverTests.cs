using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using PetrolRios.Application.Security;
using PetrolRios.Infrastructure.Hubs;

namespace PetrolRios.Api.Tests;

public sealed class AlertHubGroupResolverTests
{
    [Fact]
    public void AlertsHub_RequiresAuthentication()
    {
        typeof(AlertsHub)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Should().NotBeEmpty();
    }

    [Fact]
    public void Resolve_StationAdmin_JoinsOnlyItsStation()
    {
        var user = Principal(
            new Claim(ClaimTypes.Role, "Administrador"),
            new Claim(PetrolRiosClaimTypes.EstacionId, "7"));

        AlertHubGroupResolver.Resolve(user)
            .Should().Equal("estacion-7");
    }

    [Theory]
    [InlineData("Auditor", "auditores")]
    [InlineData("Supervisor", "supervisores")]
    [InlineData("Administrador", "administradores")]
    public void Resolve_CentralUser_JoinsRoleGroup(string role, string expected)
    {
        var user = Principal(new Claim(ClaimTypes.Role, role));

        AlertHubGroupResolver.Resolve(user)
            .Should().Equal(expected);
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));
}
