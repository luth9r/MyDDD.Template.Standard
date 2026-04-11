namespace MyDDD.Template.Infrastructure.Auth;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string Authority { get; init; } = string.Empty;
    public string MetadataAddress { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string AdminUrl { get; init; } = string.Empty;
    public string Realm { get; init; } = "my-realm";

    public string ClientId { get; init; } = string.Empty;
    public string AdminClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string AdminSecret { get; init; } = string.Empty;
}
