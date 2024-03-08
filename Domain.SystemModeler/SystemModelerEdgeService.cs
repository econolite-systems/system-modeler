// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.SystemModeller;
using Econolite.Ode.Helpers.Exceptions;
using Econolite.Ode.Model.SystemModeller;
using Econolite.OdeRepository.SystemModeller;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Domain.Configuration;

public class SystemModelerEdgeService : ISystemModelerService
{
    private readonly ILogger<SystemModelerEdgeService> _logger;
    private readonly IEntityModelJsonFileRepository _repository;

    public SystemModelerEdgeService(IEntityModelJsonFileRepository repository, ILogger<SystemModelerEdgeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<EntityModel>> GetAllAsync()
    {
        return await _repository.GetAllExceptDeletedAsync();
    }
    
    public async Task<IEnumerable<EntityModel>> GetAllByIntersectionIdAsync(Guid id)
    {
        return await _repository.GetByIntersectionIdAsync(id);
    }

    public async Task<EntityModel?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<EntityModel?> Add(EntityModel add)
    {
        var updated = EntityModelFactory.Create(add);
        _repository.Add(updated);

        var (success, errors) = await _repository.DbContext.SaveChangesAsync();

        if (!success && errors != null)
            _logger.LogInformation(errors);

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
        await Task.FromResult(false);
    }
}