// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Model.SystemModeller;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Econolite.OdeRepository.SystemModeller;

public class EntityModelRepository : GuidDocumentRepositoryBase<EntityModel>, IEntityModelRepository
{
    private static readonly double METERS_PER_MILE = 1609.34;
        
    public EntityModelRepository(IMongoContext context, ILogger<EntityModelRepository> logger) : base(context, logger)
    {
        CollectionName = "SystemModeller";
    }

    public override string CollectionName { get; protected set; }

    public async Task<IEnumerable<EntityModel>> QueryIntersectingDistanceInMiles(GeoJsonPoint point, int miles)
    {
        var coordinates = point.Coordinates.ToMongoCoordinates();
        var center = GeoJson.Point(coordinates);
        var filter = MongoDB.Driver.Builders<EntityModel>.Filter.NearSphere("geoFence", center, miles * METERS_PER_MILE);
    
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectionsWithinRadiusDistanceInMiles(GeoJsonPoint point, int miles)
    {
        var coordinates = point.Coordinates.ToMongoCoordinates();
        var center = GeoJson.Point(coordinates);

        var filter = MongoDB.Driver.Builders<EntityModel>.Filter.And(
            MongoDB.Driver.Builders<EntityModel>.Filter.Where(x => x.EntityType == "Intersection"),
            MongoDB.Driver.Builders<EntityModel>.Filter.NearSphere("geoFence", center, miles * METERS_PER_MILE));
        
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectingGeoFences(GeoJsonPoint point)
    {
        var coordinates = point.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.Point(coordinates);
        var filter = MongoDB.Driver.Builders<EntityModel>.Filter.GeoIntersects("geoFence", intersect);
    
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectingIntersections(GeoJsonLineString route)
    {
        var coordinates = route.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.LineString(coordinates);
        var filter = MongoDB.Driver.Builders<EntityModel>.Filter.And(
            MongoDB.Driver.Builders<EntityModel>.Filter.Where(x => x.EntityType == "Intersection"),
            MongoDB.Driver.Builders<EntityModel>.Filter.GeoIntersects("geoFence", intersect));
    
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectingApproaches(GeoJsonLineString route)
    {
        var coordinates = route.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.LineString(coordinates);
        var filter = MongoDB.Driver.Builders<EntityModel>.Filter.And(
            MongoDB.Driver.Builders<EntityModel>.Filter.Where(x => x.EntityType == "Approach"),
            MongoDB.Driver.Builders<EntityModel>.Filter.GeoIntersects("geometry", intersect));
    
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectingStreetSegment(GeoJsonPoint point)
    {
        var coordinates = point.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.Point(coordinates);
        var filter = MongoDB.Driver.Builders<EntityModel>.Filter.And(
            MongoDB.Driver.Builders<EntityModel>.Filter.Where(x => x.EntityType == "StreetSegment"),
            MongoDB.Driver.Builders<EntityModel>.Filter.GeoIntersects("geoFence", intersect));
    
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }

    public async Task SoftDelete(Guid id)
    {
        var toDelete = await GetByIdAsync(id);
        if (toDelete == null) return;
        toDelete.IsDeleted = true;
        Update(toDelete);
    }

    public async Task<IEnumerable<EntityModel>> GetAllExceptDeletedAsync()
    {
        var filter = 
            MongoDB.Driver.Builders<EntityModel>.Filter.Or(
                MongoDB.Driver.Builders<EntityModel>.Filter.Where(x => x.IsDeleted == false),
                MongoDB.Driver.Builders<EntityModel>.Filter.Where(x => !x.IsDeleted.HasValue));
    
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }
    
    public async Task<IEnumerable<EntityModel>> GetByIntersectionIdAsync(Guid id)
    {
        var filter = 
            MongoDB.Driver.Builders<EntityModel>.Filter.Or(
                MongoDB.Driver.Builders<EntityModel>.Filter.Eq("properties.intersection", id.ToString()),
                MongoDB.Driver.Builders<EntityModel>.Filter.Eq("_id", id.ToString()));
    
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityModel>(filter));
        return results?.ToList() ?? new List<EntityModel>();
    }
}