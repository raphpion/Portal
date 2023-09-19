﻿using Bogus;
using Logitar.Portal.Contracts.Messages;
using Logitar.Portal.Domain.Realms;
using Logitar.Portal.Domain.Users;

namespace Logitar.Portal.Domain.Messages;

[Trait(Traits.Category, Categories.Unit)]
public class RecipientsTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "It should concatenate all recipients.")]
  public void It_should_concatenate_all_recipients()
  {
    RealmAggregate realm = new("logitar");
    UserAggregate user = new(realm.UniqueNameSettings, _faker.Person.UserName, realm.Id.Value)
    {
      Email = new EmailAddress(_faker.Person.Email),
      FirstName = _faker.Person.FirstName,
      LastName = _faker.Person.LastName
    };

    var inputs = new ReadOnlyRecipient[]
    {
      new(_faker.Internet.Email(), _faker.Name.FullName()),
      ReadOnlyRecipient.From(user),
      new(_faker.Internet.Email(), _faker.Name.FullName(), RecipientType.CC),
      new(_faker.Internet.Email(), type: RecipientType.Bcc)
    };

    Recipients recipients = new(inputs);

    IEnumerable<ReadOnlyRecipient> allRecipients = recipients.AsEnumerable();
    Assert.Equal(inputs.Select(x => x.ToString()).OrderBy(x => x), allRecipients.Select(x => x.ToString()).OrderBy(x => x));
  }

  [Fact(DisplayName = "It should create an empty recipient list.")]
  public void It_should_create_an_empty_recipient_list()
  {
    Recipients recipients = new();
    Assert.Empty(recipients.Bcc);
    Assert.Empty(recipients.CC);
    Assert.Empty(recipients.To);
  }

  [Fact(DisplayName = "It should create an empty recipient list from a capacity.")]
  public void It_should_create_an_empty_recipient_list_from_a_capacity()
  {
    Recipients recipients = new(capacity: 3);
    Assert.Empty(recipients.Bcc);
    Assert.Empty(recipients.CC);
    Assert.Empty(recipients.To);
  }

  [Fact(DisplayName = "It should populate the correct list of recipients.")]
  public void It_should_populate_the_correct_list_of_recipients()
  {
    RealmAggregate realm = new("logitar");
    UserAggregate user = new(realm.UniqueNameSettings, _faker.Person.UserName, realm.Id.Value)
    {
      Email = new EmailAddress(_faker.Person.Email),
      FirstName = _faker.Person.FirstName,
      LastName = _faker.Person.LastName
    };

    var inputs = new ReadOnlyRecipient[]
    {
      new(_faker.Internet.Email(), _faker.Name.FullName()),
      ReadOnlyRecipient.From(user),
      new(_faker.Internet.Email(), _faker.Name.FullName(), RecipientType.CC),
      new(_faker.Internet.Email(), type: RecipientType.Bcc)
    };

    Recipients recipients = new(inputs);

    Assert.Equal(2, recipients.To.Count);
    Assert.Same(inputs[0], recipients.To.ElementAt(0));
    Assert.Same(inputs[1], recipients.To.ElementAt(1));
    Assert.Same(inputs[2], recipients.CC.Single());
    Assert.Same(inputs[3], recipients.Bcc.Single());
  }

  [Fact(DisplayName = "It should return an empty list of recipients.")]
  public void It_should_return_an_empty_list_of_recipients()
  {
    Recipients recipients = new();
    Assert.Empty(recipients.AsEnumerable());
  }

  [Fact(DisplayName = "It should return an empty list of recipients from a capacity.")]
  public void It_should_return_an_empty_list_of_recipients_from_a_capacity()
  {
    Recipients recipients = new(capacity: 3);
    Assert.Empty(recipients.AsEnumerable());
  }
}