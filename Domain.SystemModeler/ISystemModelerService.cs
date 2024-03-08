// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Model.SystemModeller;

namespace Econolite.Ode.Domain.Configuration;

public interface ISystemModelerService
{
    Task<IEnumerable<EntityModel>> GetAllAsync();
    Task<IEnumerable<EntityModel>> GetAllByIntersectionIdAsync(Guid id);
    Task<EntityModel> GetByIdAsync(Guid id);
    Task<EntityModel?> Add(EntityModel add);
    Task<EntityModel?> Update(EntityModel update);
    Task<bool> SoftDelete(Guid id);
    Task<bool> Delete(Guid id);
    Task PublishConfig(Guid id);
}