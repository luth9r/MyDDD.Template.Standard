using System.Text.Json.Serialization;

namespace MyDDD.Template.Application.Abstractions;

public sealed record AccessTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_expires_in")] int RefreshExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("id_token")] string? IdToken,
    [property: JsonPropertyName("session_state")] string? SessionState,
    [property: JsonPropertyName("scope")] string? Scope
);
