using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Infrastructure.Auth.Jwt;

namespace MyDDD.Template.Infrastructure.Auth;

public sealed class IdentityService(
    HttpClient httpClient,
    IOptions<KeycloakOptions> options) : IIdentityService
{
    private readonly KeycloakOptions _options = options.Value;

    public async Task<Result<AccessTokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
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
            return Result.Failure<AccessTokenResponse>(MyError.Failure("Auth.InvalidCredentials",
                "Invalid email or password"));
        }

        var result =
            await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken);
        return result ?? Result.Failure<AccessTokenResponse>(MyError.Problem("Auth.NoToken", "Token empty"));
    }

    public UserJwtInfo GetUserJwtInfo(AccessTokenResponse token)
    {
        return token.AccessToken.GetUserInfo();
    }

    public async Task<Result> UpdateUserAttributesAsync(
        string identityId,
        Dictionary<string, string[]> attributes,
        CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"admin/realms/{_options.Realm}/users/{identityId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var getResponse = await httpClient.SendAsync(getRequest, cancellationToken);
        if (!getResponse.IsSuccessStatusCode)
        {
            return Result.Failure(MyError.Failure("Keycloak.FetchError", "Could not fetch user before update"));
        }

        var userDoc =
            await getResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>(
                cancellationToken);

        if (userDoc is null)
        {
            return Result.Failure(MyError.Problem("Keycloak.EmptyUser", "User data is empty"));
        }

        var existingAttributes = userDoc.TryGetValue("attributes", out var value)
            ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(value.ToString()!)
            : new Dictionary<string, string[]>();

        foreach (var attr in attributes)
        {
            existingAttributes![attr.Key] = attr.Value;
        }

        userDoc["attributes"] = existingAttributes!;

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"admin/realms/{_options.Realm}/users/{identityId}");
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Content = JsonContent.Create(userDoc);

        var putResponse = await httpClient.SendAsync(putRequest, cancellationToken);

        return putResponse.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(MyError.Failure("Keycloak.UpdateError",
                $"Failed to PUT user. Status: {putResponse.StatusCode}"));
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

        var result = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(ct);
        return result!.AccessToken;
    }
}
