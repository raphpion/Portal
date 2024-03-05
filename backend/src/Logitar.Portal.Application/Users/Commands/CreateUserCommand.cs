﻿using Logitar.Portal.Application.Logging;
using Logitar.Portal.Contracts.Users;
using MediatR;

namespace Logitar.Portal.Application.Users.Commands;

internal record CreateUserCommand(CreateUserPayload Payload) : ApplicationRequest, IRequest<User>
{
  public override IActivity GetActivity()
  {
    if (Payload.Password == null)
    {
      return base.GetActivity();
    }

    CreateUserCommand command = this.DeepClone();
    command.Payload.Password = Payload.Password.Mask();
    return command;
  }
}