﻿using Logitar.Portal.Contracts.Senders;
using MediatR;

namespace Logitar.Portal.Application.Senders.Commands;

internal record CreateSenderCommand(CreateSenderPayload Payload) : ApplicationRequest, IRequest<Sender>;