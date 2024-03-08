// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json;
using Econolite.Ode.Domain.SystemModeller;
using Econolite.Ode.Model.SystemModeller;
using Econolite.Ode.Persistence.Common.Contexts;
using Econolite.Ode.Persistence.Mongo.Context;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace Econolite.OdeRepository.SystemModeller;

public class EntityModelEdgeRepository : IEntityModelJsonFileRepository
{
    private readonly string _path = "./data/config.json";
    private DateTime _lastLoad = DateTime.MinValue;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<EntityModelEdgeRepository> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public EntityModelEdgeRepository(IMongoContext context, IMemoryCache memoryCache, ILogger<EntityModelEdgeRepository> logger)
    {
        DbContext = context;
        _memoryCache = memoryCache;
        _logger = logger;
        CollectionName = "SystemModeller";
    }
    
    public EntityModelEdgeRepository(IMongoContext context, IMemoryCache memoryCache, ILogger<EntityModelEdgeRepository> logger, string path) : this(context, memoryCache, logger)
    {
        _path = path;
    }

    public string CollectionName { get; protected set; }

    public async Task<IEnumerable<EntityModel>> QueryIntersectingGeoFences(GeoJsonPoint point)
    {
        var models = await LoadJsonAsync();
        
        var coordinate = point.Coordinates.ToCoordinate();
        var geometry = Geometry.DefaultFactory.CreatePoint(coordinate);
        var geoFence = models.Where(m => (m.GeoFence != null)).ToArray();
        return geoFence.Where(m => m.GeoFence != null && m.GeoFence.ToPolygon().Intersects(geometry)).ToArray();
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectingIntersections(GeoJsonLineString route)
    {
        return await QueryIntersectingByType("Intersection", route);
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectingApproaches(GeoJsonLineString route)
    {
        return await QueryIntersectingGeometryLineStringByType("Approach", route);
    }
    
    public async Task<IEnumerable<EntityModel>> QueryIntersectingStreetSegment(GeoJsonPoint point)
    {
        var models = await LoadJsonAsync();
        var coordinate = point.Coordinates.ToCoordinate();
        var geometry = Geometry.DefaultFactory.CreatePoint(coordinate);
        return models
            .Where(m => m.GeoFence != null  && m.EntityType == "StreetSegment")
            .Where(m => m.GeoFence != null && m.GeoFence.ToPolygon().Intersects(geometry));
    }

    private async Task<IEnumerable<EntityModel>> QueryIntersectingByType(string type, GeoJsonLineString route)
    {
        var models = await LoadJsonAsync();
        var lineString = route.ToLinestring();
        return models
            .Where(m => m.GeoFence != null && m.EntityType == type)
            .Where(m => m.GeoFence != null && m.GeoFence.ToPolygon().Intersects(lineString));;
    }
    
    private async Task<IEnumerable<EntityModel>> QueryIntersectingGeometryLineStringByType(string type, GeoJsonLineString route)
    {
        var models = await LoadJsonAsync();
        var lineString = route.ToLinestring();
        return models
            .Where(m => m.Geometry!= null && m.EntityType == type)
            .Where(m => m.Geometry != null && m.Geometry.ToLineString().ToLinestring().Intersects(lineString));;
    }

    public async Task SoftDelete(Guid id)
    {
        var model = await GetByIdAsync(id);
        if (model != null)
        {
            model.IsDeleted = true;
            Update(model);
        }
    }

    public async Task<IEnumerable<EntityModel>> GetAllExceptDeletedAsync()
    {
        var result = await GetAllAsync();
        return result.Where(m => m.IsDeleted == null || m.IsDeleted == false);
    }
    
    public async Task<IEnumerable<EntityModel>> GetByIntersectionIdAsync(Guid id)
    {
        return await GetAllAsync();
    }

    public IDbContext DbContext { get; }
    public void Dispose()
    {
        DbContext.Dispose();
    }

    public void Add(EntityModel document)
    {
        var models = LoadJson().ToList();
        models.Add(document);
        SaveJson(models);
    }

    public async Task<EntityModel?> GetByIdAsync(Guid id)
    {
        var models = await LoadJsonAsync();
        return models.FirstOrDefault(m => m.Id == id) ?? new EntityModel();
    }

    public EntityModel GetById(Guid id)
    {
        return LoadJson().FirstOrDefault(m => m.Id == id) ?? new EntityModel();
    }

    public async Task<IEnumerable<EntityModel>> GetAllAsync()
    {
        return await LoadJsonAsync();
    }

    public IEnumerable<EntityModel> GetAll()
    {
        return LoadJson();
    }

    public void Update(EntityModel document)
    {
        var models = LoadJson();
        var modelsArray = models.ToList();
        var index = modelsArray.FindIndex(m => m.Id == document.Id);
        modelsArray[index] = document;
        SaveJson(modelsArray);
    }

    public void Remove(Guid id)
    {
        var models = LoadJson();
        var result = models.Where(m => m.Id != id);
        SaveJson(result);
    }

    public async Task ReplaceDataAsync(IEnumerable<EntityModel> models)
    {
        await SaveJsonAsync(models);
    }
    
    public async Task<IEnumerable<EntityModel>> LoadJsonAsync()
    {       
        return await _memoryCache.GetOrCreateAsync(
        "models",
        cacheEntry =>
        {
            cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(10);
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            return LoadDataAsync();
        });
    }

    public async Task<IEnumerable<EntityModel>> LoadDataAsync()
    {
        _logger.LogInformation("Loading Data");
        IEnumerable<EntityModel>? result = null;
        using StreamReader r = new StreamReader(_path);
        var json = await r.ReadToEndAsync();
        result = JsonSerializer.Deserialize<IEnumerable<EntityModel>>(json, _jsonSerializerOptions);
        return result ?? new List<EntityModel>();
    }

    public IEnumerable<EntityModel> LoadJson()
    {
        IEnumerable<EntityModel>? result = null;
        using StreamReader r = new StreamReader(_path);
        var json = r.ReadToEnd();
        result = JsonSerializer.Deserialize<IEnumerable<EntityModel>>(json, _jsonSerializerOptions);
        return result ?? new List<EntityModel>();;
    }
    
    public async Task SaveJsonAsync(IEnumerable<EntityModel> models)
    {
        var json = JsonSerializer.Serialize<IEnumerable<EntityModel>>(models, _jsonSerializerOptions);
        using StreamWriter w = new StreamWriter(_path);
        await w.WriteAsync(json);
    }
    
    private void SaveJson(IEnumerable<EntityModel> models)
    {
        var json = JsonSerializer.Serialize<IEnumerable<EntityModel>>(models, _jsonSerializerOptions);
        using StreamWriter w = new StreamWriter(_path);
        w.Write(json);
    }
}