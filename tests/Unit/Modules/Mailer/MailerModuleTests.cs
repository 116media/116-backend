using _116.Mailer;
using _116.Mailer.Application.Notifications;
using _116.Mailer.Application.Notifications.Messages;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Repositories;
using _116.Mailer.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Quartz;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer;

/// <summary>
/// Unit tests for <see cref="MailerModule" />.
/// Covers the module's service registrations and the <c>EMAIL_PROVIDER</c>
/// switch that selects the delivery adapter at boot.
/// </summary>
[Collection("EnvironmentVariable")]
public class MailerModuleTests : IDisposable
{
    private const string EmailProviderVariable = "EMAIL_PROVIDER";

    private readonly string? _previousProvider;
    private readonly ServiceCollection _services;

    public MailerModuleTests()
    {
        _previousProvider = Environment.GetEnvironmentVariable(EmailProviderVariable);
        Environment.SetEnvironmentVariable(EmailProviderVariable, null);

        _services = [];
        _services.AddLogging();
        _services.AddLocalization();
        _services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(EmailProviderVariable, _previousProvider);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Builds a host environment stub reporting the given name.
    /// </summary>
    /// <param name="name">The environment name the stub reports.</param>
    /// <returns>The stubbed host environment.</returns>
    private static IHostEnvironment HostEnvironment(string name)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(host => host.EnvironmentName).Returns(name);

        return environment.Object;
    }

    private ServiceDescriptor GetDescriptor<TService>()
    {
        ServiceDescriptor? descriptor = _services.LastOrDefault(service => service.ServiceType == typeof(TService));
        descriptor.Should().NotBeNull();

        return descriptor;
    }

    #region Service Registrations

    [Fact]
    public void AddMailerModule_ShouldRegisterTheMailerDbContext()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        _services.Should().Contain(service => service.ServiceType == typeof(MailerDbContext));
    }

    [Fact]
    public void AddMailerModule_ShouldRegisterTheUnitOfWork()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        ServiceDescriptor descriptor = GetDescriptor<IMailerUnitOfWork>();
        descriptor.ImplementationType.Should().Be<MailerUnitOfWork>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMailerModule_ShouldRegisterTheRepositories()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        GetDescriptor<IOutboxEmailRepository>().ImplementationType.Should().Be<OutboxEmailRepository>();
        GetDescriptor<INewsletterRepository>().ImplementationType.Should().Be<NewsletterRepository>();
        GetDescriptor<INotificationRepository>().ImplementationType.Should().Be<NotificationRepository>();
    }

    [Fact]
    public void AddMailerModule_ShouldRegisterTheRenderersWithTheirMessageSources()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        GetDescriptor<IEmailTemplateRenderer>().ImplementationType.Should().Be<EmailTemplateRenderer>();
        GetDescriptor<INotificationRenderer>().ImplementationType.Should().Be<NotificationRenderer>();
        _services.Should().Contain(service => service.ServiceType == typeof(EmailTemplateMessage));
        _services.Should().Contain(service => service.ServiceType == typeof(NotificationMessage));
    }

    [Fact]
    public void AddMailerModule_ShouldRegisterTheErrorFactoriesWithTheirMessageSources()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        _services.Should().Contain(service => service.ServiceType == typeof(NewsletterErrors));
        _services.Should().Contain(service => service.ServiceType == typeof(NewsletterErrorMessage));
        _services.Should().Contain(service => service.ServiceType == typeof(NotificationErrors));
        _services.Should().Contain(service => service.ServiceType == typeof(NotificationErrorMessage));
    }

    [Fact]
    public void AddMailerModule_ShouldRegisterTheMailerContract()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        ServiceDescriptor descriptor = GetDescriptor<IMailer>();
        descriptor.ImplementationType.Should().Be<OutboxMailer>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMailerModule_ShouldRegisterTheNotifierContract()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        ServiceDescriptor descriptor = GetDescriptor<INotifier>();
        descriptor.ImplementationType.Should().Be<Notifier>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMailerModule_ShouldScheduleTheOutboxDispatcherJob()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        _services.Should().Contain(service => service.ServiceType == typeof(ISchedulerFactory));
    }

    [Fact]
    public void AddMailerModule_ShouldReturnServiceCollection()
    {
        // Act
        IServiceCollection result = _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    #endregion

    #region Email Provider Selection

    [Fact]
    public void AddMailerModule_WithoutAProviderSet_ShouldFallBackToSmtp()
    {
        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        GetDescriptor<IEmailSender>().ImplementationType.Should().Be<SmtpEmailSender>();
    }

    [Fact]
    public void AddMailerModule_WithTheSmtpProvider_ShouldRegisterTheSmtpSender()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EmailProviderVariable, MailerConstants.EmailProviders.Smtp);

        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        GetDescriptor<IEmailSender>().ImplementationType.Should().Be<SmtpEmailSender>();
    }

    [Fact]
    public void AddMailerModule_WithAnUppercaseProvider_ShouldStillMatchTheBranch()
    {
        // Arrange — the switch lowercases the value, so casing never decides delivery.
        Environment.SetEnvironmentVariable(EmailProviderVariable, "SMTP");

        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        GetDescriptor<IEmailSender>().ImplementationType.Should().Be<SmtpEmailSender>();
    }

    [Fact]
    public void AddMailerModule_WithTheResendProvider_ShouldRegisterTheResendSenderAsATypedClient()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EmailProviderVariable, MailerConstants.EmailProviders.Resend);

        // Act
        _services.AddMailerModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var sender = serviceProvider.GetService<IEmailSender>();
        sender.Should().NotBeNull();
        sender.Should().BeOfType<ResendEmailSender>();
    }

    [Fact]
    public void AddMailerModule_WithAnUnknownProvider_ShouldFailAtRegistration()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EmailProviderVariable, "carrier-pigeon");

        // Act
        Action act = () => _services.AddMailerModule(HostEnvironment("Testing"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Unknown EMAIL_PROVIDER 'carrier-pigeon'.");
    }

    #endregion

    #region Pipeline

    [Fact]
    public void UseMailerModule_WithTestingEnvironment_ShouldReturnTheApplicationBuilder()
    {
        // Arrange — Testing disables migrations and the module never seeds, so
        // UseModuleDatabase is a no-op and the builder is returned untouched.
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(HostEnvironment("Testing"));

        var appBuilder = new Mock<IApplicationBuilder>();
        appBuilder.Setup(builder => builder.ApplicationServices).Returns(services.BuildServiceProvider());

        // Act
        IApplicationBuilder result = appBuilder.Object.UseMailerModule();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(appBuilder.Object);
    }

    #endregion
}
