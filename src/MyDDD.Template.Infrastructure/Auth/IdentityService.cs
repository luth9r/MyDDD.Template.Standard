using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Infrastructure.Auth;

public sealed class IdentityService(
    HttpClient httpClient,
    IOptions<KeycloakOptions> options) : IIdentityService
{
    private readonly KeycloakOptions _options = options.Value;

    public async Task<Result<string>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        Guid internalUserId,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var userRequest = new
        {
            username = email,
            email = email,
            firstName = firstName,
            lastName = lastName,
            emailVerified = false,
            enabled = true,
            attributes = new Dictionary<string, string[]>
            {
                ["userId"] = [internalUserId.ToString()],
            },
            credentials = new[] { new { type = "password", value = password, temporary = false } },
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"admin/realms/{_options.Realm}/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        request.Content = JsonContent.Create(userRequest);

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return Result.Failure<string>(Error.Conflict("Keycloak.UserExists", "User already exists"));
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<string>(Error.Failure("Keycloak.Error", await response.Content.ReadAsStringAsync(cancellationToken)));
        }

        var identityId = response.Headers.Location?.Segments.Last();
        return string.IsNullOrEmpty(identityId)
            ? Result.Failure<string>(Error.Problem("Keycloak.NoId", "ID not found"))
            : Result.Success(identityId);
    }

    public async Task<Result<AccessTokenResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"]     = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "password",
            ["username"] = email,
            ["password"] = password,
            ["scope"] = "openid profile email",
        });

        var response = await httpClient.PostAsync(
            $"realms/{_options.Realm}/protocol/openid-connect/token", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<AccessTokenResponse>(Error.Failure("Auth.InvalidCredentials", "Invalid email or password"));
        }

        var result = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: cancellationToken);
        return result ?? Result.Failure<AccessTokenResponse>(Error.Problem("Auth.NoToken", "Token empty"));
    }

    public async Task<Result> SendVerificationEmailAsync(string identityId, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Put,
            $"admin/realms/{_options.Realm}/users/{identityId}/execute-actions-email");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Keycloak expects a JSON array of strings for the required actions
        request.Content = JsonContent.Create(new[] { "VERIFY_EMAIL" });

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure(Error.Failure("Keycloak.Error", await response.Content.ReadAsStringAsync(cancellationToken)));
        }

        return Result.Success();
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.AdminClientId,
            ["client_secret"] = _options.AdminSecret,
            ["grant_type"] = "client_credentials",
        });

        var response = await httpClient.PostAsync(
            $"realms/{_options.Realm}/protocol/openid-connect/token", content, ct);

        var result = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: ct);
        return result!.AccessToken;
    }
}
