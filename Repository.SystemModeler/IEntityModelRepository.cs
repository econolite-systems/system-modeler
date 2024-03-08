// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Model.SystemModeller;
using Econolite.Ode.Persistence.Common.Repository;

namespace Econolite.OdeRepository.SystemModeller;

public interface IEntityModelRepository : IRepository<EntityModel, Guid>
{
    Task<IEnumerable<EntityModel>> GetAllExceptDeletedAsync();
    Task<IEnumerable<EntityModel>> QueryIntersectingGeoFences(GeoJsonPoint point);
    Task<IEnumerable<EntityModel>> GetByIntersectionIdAsync(Guid id);
    Task<IEnumerable<EntityModel>> QueryIntersectingIntersections(GeoJsonLineString route);
    Task<IEnumerable<EntityModel>> QueryIntersectingApproaches(GeoJsonLineString route);
    Task<IEnumerable<EntityModel>> QueryIntersectingStreetSegment(GeoJsonPoint point);
    Task SoftDelete(Guid id);
}