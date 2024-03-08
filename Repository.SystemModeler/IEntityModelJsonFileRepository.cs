// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Model.SystemModeller;

namespace Econolite.OdeRepository.SystemModeller;

public interface IEntityModelJsonFileRepository : IEntityModelRepository
{
    Task<IEnumerable<EntityModel>> LoadDataAsync();
    Task SaveJsonAsync(IEnumerable<EntityModel> models);
}