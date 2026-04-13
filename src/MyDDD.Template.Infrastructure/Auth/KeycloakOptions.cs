using System.ComponentModel.DataAnnotations;

namespace MyDDD.Template.Infrastructure.Auth;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required]
    [Url]
    public string Authority { get; init; } = string.Empty;

    [Required]
    [Url]
    public string MetadataAddress { get; init; } = string.Empty;

    [Required]
    [Url]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [Url]
    public string AdminUrl { get; init; } = string.Empty;

    [Required]
    public string Realm { get; init; } = "my-realm";

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string AdminClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    [Required]
    public string AdminSecret { get; init; } = string.Empty;
}
