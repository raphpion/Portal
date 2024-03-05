﻿using Logitar.Portal.Contracts.Dictionaries;
using MediatR;

namespace Logitar.Portal.Application.Dictionaries.Commands;

internal record DeleteDictionaryCommand(Guid Id) : ApplicationRequest, IRequest<Dictionary?>;