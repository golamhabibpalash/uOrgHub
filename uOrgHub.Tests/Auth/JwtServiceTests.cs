using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Auth.Services;

namespace uOrgHub.Tests.Auth;

public class JwtServiceTests
{
    private static readonly JwtService Service = new(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = new string('k', 64),
            ["JwtSettings:Issuer"] = "uOrgHub",
            ["JwtSettings:Audience"] = "uOrgHub",
            ["JwtSettings:AccessTokenExpiryMinutes"] = "15",
        })
        .Build());

    private static readonly ApplicationUser User = new()
    {
        Id = Guid.NewGuid(),
        Username = "F.Chowdhury",
        Email = "someone@example.com",
        FirstName = "Test",
        LastName = "User",
    };

    [Fact]
    public void Access_token_carries_identity_roles_and_company_but_no_permissions()
    {
        var companyId = Guid.NewGuid();

        var token = new JwtSecurityTokenHandler().ReadJwtToken(
            Service.GenerateAccessToken(User, [Roles.Admin, Roles.Accountant], companyId));

        token.Claims.Should().NotContain(c => c.Type == "permission");
        token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .Should().BeEquivalentTo(Roles.Admin, Roles.Accountant);
        token.Claims.Should().Contain(c => c.Type == "company_id" && c.Value == companyId.ToString());
        token.Claims.Should().Contain(c => c.Type == "username" && c.Value == "F.Chowdhury");
    }

    [Fact]
    public void Access_token_for_a_user_holding_every_role_fits_well_under_proxy_header_limits()
    {
        // Regression: with permissions embedded, a four-role user's header reached ~9 KB and nginx's
        // default 8 KB limit rejected every request after login.
        var allRoles = AuthorizationCatalog.AllRoles.Select(r => r.Name).ToList();

        var header = "Bearer " + Service.GenerateAccessToken(User, allRoles, Guid.NewGuid());

        header.Length.Should().BeLessThan(2048);
    }
}
