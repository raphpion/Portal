﻿using Logitar.Portal.Contracts.Sessions;
using MediatR;

namespace Logitar.Portal.Application.Sessions.Commands;

internal record SignOutSessionCommand(Guid Id) : ApplicationRequest, IRequest<Session?>;