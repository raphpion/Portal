﻿using Logitar.Portal.Contracts.Users;
using MediatR;

namespace Logitar.Portal.Application.Users.Commands;

internal record SignOutUserCommand(Guid Id) : ApplicationRequest, IRequest<User?>;