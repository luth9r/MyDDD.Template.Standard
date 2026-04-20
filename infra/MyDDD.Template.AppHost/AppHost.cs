using System.Globalization;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Parameters
var clientSecret = builder.AddParameter("KeycloakClientSecret", "my-client-secret", secret: true);
var adminSecret = builder.AddParameter("KeycloakAdminSecret", "my-admin-secret", secret: true);
var dbUser = builder.AddParameter("DbUser", "user");
var dbPassword = builder.AddParameter("dbpassword", "password", secret: true);

// Infrastructure
var isTesting = builder.Environment.IsEnvironment("Testing");
var cache = builder.AddRedis("cache")
    .WithRedisInsight();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();
if (!isTesting) rabbitmq.WithDataVolume("rabbitmq-data");

var postgres = builder.AddPostgres("postgres", password: dbPassword, userName: dbUser)
    .WithEndpoint(targetPort: 5432, port: 5432, name: "external", isProxied: false);
if (!isTesting) postgres.WithDataVolume("postgres-data");

var dbName = builder.Configuration["Database:Name"] ?? "myddd-db";
var db = postgres.AddDatabase(dbName);

var seq = builder.AddContainer("seq", "datalust/seq")
    .WithHttpEndpoint(targetPort: 80, port: 5341, name: "http")
    .WithEndpoint(targetPort: 4317, port: 4317, name: "otlp")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SEQ_FIRSTRUN_NOAUTHENTICATION", "true");

var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(8025, 8025, "ui")
    .WithEndpoint(1025, 1025, name: "smtp");

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak")
    .WithArgs("start-dev", "--import-realm")
    .WithHttpEndpoint(targetPort: 8080, port: 8080, name: "http")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_HTTP_MANAGEMENT_PORT", "9000")
    // SMTP Configuration for MailHog
    .WithEnvironment("KC_MAIL_SMTP_HOST", "mailhog")
    .WithEnvironment("KC_MAIL_SMTP_PORT", "1025")
    .WithEnvironment("KC_MAIL_SMTP_FROM", "no-reply@myddd-template.com")
    .WithEnvironment("KC_MAIL_SMTP_FROM_DISPLAY_NAME", "MyDDD Template")
    .WithBindMount("./Realms", "/opt/keycloak/data/import");

if (!isTesting)
{
    keycloak.WithVolume("keycloak-data", "/opt/keycloak/data");
}

builder.AddProject<Projects.MyDDD_Template_Api>("api")
    .WithHttpEndpoint(5000, name: "http")
    .WithReference(db)
    .WithReference(cache)
    .WithReference(rabbitmq)
    .WaitFor(db)
    .WaitFor(keycloak)
    .WaitFor(rabbitmq)
    .WaitFor(mailhog)
    .WaitFor(seq)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Keycloak__Authority", () => $"{keycloak.GetEndpoint("http").Url}/realms/my-realm")
    .WithEnvironment("Keycloak__MetadataAddress",
        () => $"{keycloak.GetEndpoint("http").Url}/realms/my-realm/.well-known/openid-configuration")
    .WithEnvironment("Keycloak__Issuer", () => $"{keycloak.GetEndpoint("http").Url}/realms/my-realm")
    .WithEnvironment("Keycloak__Realm", "my-realm")
    .WithEnvironment("Keycloak__AdminUrl", () => $"{keycloak.GetEndpoint("http").Url}")
    .WithEnvironment("Keycloak__ClientId", "scalar-client")
    .WithEnvironment("Keycloak__AdminClientId", "api-client")
    .WithEnvironment("Keycloak__ClientSecret", clientSecret)
    .WithEnvironment("Keycloak__AdminSecret", adminSecret)
    .WithEnvironment("Keycloak__Audience", "account")
    .WithEnvironment("Seq__ServerUrl", () => seq.GetEndpoint("http").Url)
    .WithEnvironment("MailHog__Smtp__Host", () => mailhog.GetEndpoint("smtp").Host)
    .WithEnvironment("MailHog__Smtp__Port",
        () => mailhog.GetEndpoint("smtp").Port.ToString(CultureInfo.InvariantCulture));

builder.Build().Run();
