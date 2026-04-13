using System.IdentityModel.Tokens.Jwt;
using MyDDD.Template.Application.Abstractions;

namespace MyDDD.Template.Infrastructure.Auth.Jwt;

internal static class JwtExtensions
{
    public static UserJwtInfo GetUserInfo(this string accessToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        return new UserJwtInfo(
            IdentityId: jwt.Subject,
            Email: jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
            IsEmailVerified: jwt.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value == "true",
            InternalUserId: Guid.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "userId")?.Value, out var id) ? id : null,
            FirstName: jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ?? string.Empty,
            LastName: jwt.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value ?? string.Empty
        );
    }
}
