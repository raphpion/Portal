﻿using Logitar.Portal.Contracts.Users;
using MediatR;

namespace Logitar.Portal.Application.Users.Commands;

internal record RemoveUserIdentifierCommand(Guid Id, string Key) : ApplicationRequest, IRequest<User?>;