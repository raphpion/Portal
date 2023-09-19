﻿using Bogus;
using Logitar.EventSourcing;
using Logitar.EventSourcing.EntityFrameworkCore.Relational;
using Logitar.Portal.Application;
using Logitar.Portal.Application.Logging;
using Logitar.Portal.Contracts;
using Logitar.Portal.Contracts.Actors;
using Logitar.Portal.Contracts.Configurations;
using Logitar.Portal.Contracts.Sessions;
using Logitar.Portal.Domain;
using Logitar.Portal.Domain.Configurations;
using Logitar.Portal.Domain.Passwords;
using Logitar.Portal.Domain.Sessions;
using Logitar.Portal.Domain.Users;
using Logitar.Portal.EntityFrameworkCore.PostgreSQL;
using Logitar.Portal.EntityFrameworkCore.Relational;
using Logitar.Portal.EntityFrameworkCore.Relational.Entities;
using Logitar.Portal.Infrastructure;
using Logitar.Portal.Infrastructure.Passwords.Strategies;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logitar.Portal;

public abstract class IntegrationTests
{
  protected const string PasswordString = "Test123!";

  private readonly TestApplicationContext _applicationContext = new();
  private readonly ILoggingService _loggingService;

  protected IntegrationTests()
  {
    IConfiguration configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
      .AddUserSecrets("6ca0768e-971e-4542-b972-5d74f0e35f49")
      .Build();

    ServiceCollection services = new();
    services.AddMemoryCache();
    services.AddSingleton(configuration);
    services.AddSingleton<IApplicationContext>(_applicationContext);

    string connectionString;
    DatabaseProvider databaseProvider = configuration.GetValue<DatabaseProvider?>("DatabaseProvider")
      ?? DatabaseProvider.EntityFrameworkCorePostgreSQL;
    switch (databaseProvider)
    {
      case DatabaseProvider.EntityFrameworkCorePostgreSQL:
        connectionString = (configuration.GetValue<string>("POSTGRESQLCONNSTR_Portal")?.Replace("{database}", GetType().Name)) ?? string.Empty;
        services.AddLogitarPortalWithEntityFrameworkCorePostgreSQL(connectionString);
        break;
    }

    services.AddSingleton<IPbkdf2Settings>(_ => new Pbkdf2Settings { Iterations = 6 });
    ServiceProvider = services.BuildServiceProvider();

    AggregateRepository = ServiceProvider.GetRequiredService<IAggregateRepository>();
    PasswordService = ServiceProvider.GetRequiredService<IPasswordService>();

    EventContext = ServiceProvider.GetRequiredService<EventContext>();
    PortalContext = ServiceProvider.GetRequiredService<PortalContext>();

    _loggingService = ServiceProvider.GetRequiredService<ILoggingService>();
    _loggingService.Start();
  }

  protected Faker Faker { get; } = new();
  protected ReadOnlyLocale Locale => new(Faker.Locale);
  protected bool InitializeConfiguration { get; set; } = true;

  protected IServiceProvider ServiceProvider { get; }
  protected IAggregateRepository AggregateRepository { get; }
  protected IPasswordService PasswordService { get; }

  protected EventContext EventContext { get; }
  protected PortalContext PortalContext { get; }

  protected ConfigurationAggregate? Configuration { get; private set; }
  protected UserAggregate? User { get; private set; }
  protected SessionAggregate? Session { get; private set; }
  protected Actor Actor { get; private set; } = new();
  protected ActorId ActorId => new(Actor.Id);

  public virtual async Task InitializeAsync()
  {
    IPublisher publisher = ServiceProvider.GetRequiredService<IPublisher>();
    await publisher.Publish(new InitializeDatabaseCommand());

    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Actors"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"UPDATE ""Realms"" SET ""PasswordRecoveryTemplateId"" = NULL;");
    await PortalContext.Database.ExecuteSqlRawAsync(@"UPDATE ""Realms"" SET ""PasswordRecoverySenderId"" = NULL;");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Messages"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Templates"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Senders"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Dictionaries"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Sessions"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Users"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""ApiKeys"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Roles"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Realms"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Logs"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""TokenBlacklist"";");
    await PortalContext.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Events"";");

    if (InitializeConfiguration)
    {
      await InitializeConfigurationAsync();
    }
  }
  public virtual Task DisposeAsync() => Task.CompletedTask;

  protected virtual async Task InitializeConfigurationAsync()
  {
    IConfigurationService configurationService = ServiceProvider.GetRequiredService<IConfigurationService>();

    InitializeConfigurationPayload payload = new()
    {
      Locale = Faker.Locale,
      User = new UserPayload
      {
        UniqueName = Faker.Person.UserName,
        Password = PasswordString,
        EmailAddress = Faker.Person.Email,
        FirstName = Faker.Person.FirstName,
        LastName = Faker.Person.LastName
      },
      Session = new SessionPayload
      {
        CustomAttributes = new CustomAttribute[]
        {
          new("IpAddress", Faker.Internet.IpAddress().ToString()),
          new("UserAgent", Faker.Internet.UserAgent())
        }
      }
    };
    InitializeConfigurationResult result = await configurationService.InitializeAsync(payload);
    Session session = result.Session;

    Configuration = await AggregateRepository.LoadAsync<ConfigurationAggregate>(ConfigurationAggregate.UniqueId);
    Assert.NotNull(Configuration);
    _applicationContext.Configuration = Configuration;

    User = await AggregateRepository.LoadAsync<UserAggregate>(new AggregateId(session.User.Id));
    Assert.NotNull(User);

    Session = await AggregateRepository.LoadAsync<SessionAggregate>(new AggregateId(session.Id));
    Assert.NotNull(Session);

    Actor = new()
    {
      Id = User.Id.ToGuid(),
      Type = ActorType.User,
      DisplayName = User.FullName ?? User.UniqueName,
      EmailAddress = User.Email?.Address
    };
    _applicationContext.ActorId = ActorId;

    _applicationContext.BaseUrl = new Uri($"https://portal.{Faker.Internet.DomainName()}/");
  }

  protected static void AssertIsNear(DateTime? value, int seconds = 60)
  {
    Assert.True(value.HasValue);
    AssertIsNear(value.Value, seconds);
  }
  protected static void AssertIsNear(DateTime value, int seconds = 60)
  {
    TimeSpan difference = (DateTime.UtcNow - value.ToUniversalTime()).Duration();
    Assert.True(difference < TimeSpan.FromSeconds(seconds));
  }

  protected async Task AssertUserPasswordAsync(Guid userId, string? passwordString = null)
  {
    AggregateId aggregateId = new(userId);
    UserEntity? user = await PortalContext.Users.AsNoTracking()
      .SingleOrDefaultAsync(x => x.AggregateId == aggregateId.Value);
    Assert.NotNull(user);

    Assert.NotNull(user.Password);
    Password password = PasswordService.Decode(user.Password);
    Assert.True(password.IsMatch(passwordString ?? PasswordString));
  }

  protected static void AssertEqual(DateTime? expected, DateTime? actual)
  {
    if (expected.HasValue)
    {
      Assert.True(actual.HasValue);
      Assert.Equal(ToUnixTimeMilliseconds(expected.Value), ToUnixTimeMilliseconds(actual.Value));
    }
    else
    {
      Assert.Null(actual);
    }
  }
  protected static long? ToUnixTimeMilliseconds(DateTime? value)
    => value.HasValue ? new DateTimeOffset(value.Value).ToUnixTimeMilliseconds() : null;
}