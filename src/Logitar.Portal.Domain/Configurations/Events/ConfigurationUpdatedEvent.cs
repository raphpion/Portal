﻿using Logitar.EventSourcing;
using Logitar.Portal.Domain.Settings;
using MediatR;

namespace Logitar.Portal.Domain.Configurations.Events;

public record ConfigurationUpdatedEvent : DomainEvent, INotification
{
  public ReadOnlyLocale? DefaultLocale { get; set; }
  public JwtSecret? Secret { get; set; }

  public ReadOnlyUniqueNameSettings? UniqueNameSettings { get; set; }
  public ReadOnlyPasswordSettings? PasswordSettings { get; set; }

  public ReadOnlyLoggingSettings? LoggingSettings { get; set; }
}