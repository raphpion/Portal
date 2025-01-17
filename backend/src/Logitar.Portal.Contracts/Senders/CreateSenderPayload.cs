﻿namespace Logitar.Portal.Contracts.Senders;

public record CreateSenderPayload
{
  public string? EmailAddress { get; set; }
  public string? PhoneNumber { get; set; }
  public string? DisplayName { get; set; }
  public string? Description { get; set; }

  public MailgunSettings? Mailgun { get; set; }
  public SendGridSettings? SendGrid { get; set; }
  public TwilioSettings? Twilio { get; set; }

  public CreateSenderPayload() : this(string.Empty)
  {
  }

  public CreateSenderPayload(string emailAddress)
  {
    EmailAddress = emailAddress;
  }
}
