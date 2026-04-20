using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDDD.Template.Application.Projects.CreateProject;
using MyDDD.Template.Infrastructure.Behaviors;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace MyDDD.Template.Infrastructure.Extensions;

internal static class MessagingExtensions
{
    public static IHostApplicationBuilder AddMessaging(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.UseWolverine(opts =>
        {
            var rabbitMqConnectionString = configuration.GetConnectionString("rabbitmq");
            if (!string.IsNullOrEmpty(rabbitMqConnectionString))
            {
                opts.UseRabbitMq(new Uri(rabbitMqConnectionString)).AutoProvision();
            }

            opts.PersistMessagesWithPostgresql(configuration.GetConnectionString("myddd-db")!);
            opts.UseEntityFrameworkCoreTransactions();
            opts.Policies.UseDurableLocalQueues();

            opts.ListenToRabbitQueue("integration-events");

            opts.Discovery.IncludeAssembly(typeof(CreateProjectCommand).Assembly);

            // Official Wolverine FluentValidation — discovers and registers validators automatically
            opts.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);

            // Use our custom failure action that returns domain Result instead of throwing ValidationException
            opts.Services.AddSingleton(typeof(IFailureAction<>), typeof(ValidationFailureAction<>));
        });

        return builder;
    }
}
