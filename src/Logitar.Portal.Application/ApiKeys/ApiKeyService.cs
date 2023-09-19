﻿using Logitar.Portal.Application.ApiKeys.Commands;
using Logitar.Portal.Application.ApiKeys.Queries;
using Logitar.Portal.Contracts;
using Logitar.Portal.Contracts.ApiKeys;

namespace Logitar.Portal.Application.ApiKeys;

internal class ApiKeyService : IApiKeyService
{
  private readonly IRequestPipeline _pipeline;

  public ApiKeyService(IRequestPipeline pipeline)
  {
    _pipeline = pipeline;
  }

  public async Task<ApiKey> AuthenticateAsync(string xApiKey, CancellationToken cancellationToken)
  {
    return await _pipeline.ExecuteAsync(new AuthenticateApiKeyCommand(xApiKey), cancellationToken);
  }

  public async Task<ApiKey> CreateAsync(CreateApiKeyPayload payload, CancellationToken cancellationToken)
  {
    return await _pipeline.ExecuteAsync(new CreateApiKeyCommand(payload), cancellationToken);
  }

  public async Task<ApiKey?> DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _pipeline.ExecuteAsync(new DeleteApiKeyCommand(id), cancellationToken);
  }

  public async Task<ApiKey?> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _pipeline.ExecuteAsync(new ReadApiKeyQuery(id), cancellationToken);
  }

  public async Task<ApiKey?> ReplaceAsync(Guid id, ReplaceApiKeyPayload payload, long? version, CancellationToken cancellationToken)
  {
    return await _pipeline.ExecuteAsync(new ReplaceApiKeyCommand(id, payload, version), cancellationToken);
  }

  public async Task<SearchResults<ApiKey>> SearchAsync(SearchApiKeysPayload payload, CancellationToken cancellationToken)
  {
    return await _pipeline.ExecuteAsync(new SearchApiKeysQuery(payload), cancellationToken);
  }

  public async Task<ApiKey?> UpdateAsync(Guid id, UpdateApiKeyPayload payload, CancellationToken cancellationToken)
  {
    return await _pipeline.ExecuteAsync(new UpdateApiKeyCommand(id, payload), cancellationToken);
  }
}