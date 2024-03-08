// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json;
using Econolite.Ode.Domain.SystemModeller;
using Econolite.Ode.Helpers.Exceptions;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Model.SystemModeller;
using Econolite.OdeRepository.SystemModeller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Econolite.Ode.Domain.Configuration;

public class SystemModelerService : ISystemModelerService
{
    private readonly ILogger<SystemModelerService> _logger;
    private readonly IEntityModelRepository _repository;
    private readonly IProducer<Guid, GenericJsonResponse> _producer;
    private readonly MessageFactory<Guid, GenericJsonResponse> _messageFactory;
    private readonly string _producerTopic;
    
    public SystemModelerService(IConfiguration configuration, IEntityModelRepository repository, IProducer<Guid, GenericJsonResponse> producer, MessageFactory<Guid, GenericJsonResponse> messageFactory, ILogger<SystemModelerService> logger)
    {
        _repository = repository;
        _producer = producer;
        _messageFactory = messageFactory;
        _logger = logger;
        _producerTopic = configuration["Topics:ConfigPriorityResponse"];
    }

    public async Task<IEnumerable<EntityModel>> GetAllAsync()
    {
        return await _repository.GetAllExceptDeletedAsync();
    }

    public async Task<EntityModel?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<EntityModel>> GetAllByIntersectionIdAsync(Guid id)
    {
        return await _repository.GetByIntersectionIdAsync(id);
    }
    
    public async Task<EntityModel?> Add(EntityModel add)
    {
        var updated = EntityModelFactory.Create(add);
        _repository.Add(updated);

        var (success, errors) = await _repository.DbContext.SaveChangesAsync();

        if (!success) _logger.LogInformation(errors);
        await SendUpdate(updated);
        
        return updated;
    }

    public async Task<EntityModel?> Update(EntityModel update)
    {
        try
        {
            var updated = EntityModelFactory.Create(update);
            _repository.Update(updated);
            var (success, errors) = await _repository.DbContext.SaveChangesAsync();
            if (!success && !string.IsNullOrWhiteSpace(errors)) throw new UpdateException(errors);
            
            await SendUpdate(updated);
            
            return updated;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task<bool> Delete(Guid id)
    {
        try
        {
            var signal = await _repository.GetByIdAsync(id);

            _repository.Remove(id);
            var (success, errors) = await _repository.DbContext.SaveChangesAsync();
            await SendUpdate(signal);
            return success;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
    
    public async Task<bool> SoftDelete(Guid id)
    {
        try
        {
            var signal = await _repository.GetByIdAsync(id);

            await _repository.SoftDelete(id);
            var (success, errors) = await _repository.DbContext.SaveChangesAsync();
            await SendUpdate(signal);
            return success;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
    
    public async Task<bool> SoftDeleteByIntersection(Guid id)
    {
        try
        {
            var models = await _repository.GetByIntersectionIdAsync(id);

            foreach (var model in models)
            {
                await _repository.SoftDelete(model.Id);
            }
            
            var (success, errors) = await _repository.DbContext.SaveChangesAsync();

            return success;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
    
    public async Task PublishConfig(Guid id)
    {
        var results = await GetAllByIntersectionIdAsync(id);
        
        var json = JsonSerializer.Serialize<IEnumerable<EntityModel>>(results, JsonPayloadSerializerOptions.Options);
        await _producer.ProduceAsync(TopicWithDeviceId(id), _messageFactory.Build(_ => id, id, new EntityModelJsonConfigResponse(json)));
    }

    private string TopicWithDeviceId(Guid id)
    {
        return $"{_producerTopic}.{id.ToString()}";
    }

    private async Task SendUpdate(EntityModel model)
    {
        var id = model.GetIntersectionId();
        if (id != null)
        {
            await PublishConfig(id.GetValueOrDefault());
        }
    }
}