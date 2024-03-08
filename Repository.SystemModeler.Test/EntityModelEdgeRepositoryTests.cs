// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

// ReSharper disable HeapView.BoxingAllocation
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Econolite.Ode.Model.SystemModeller;
using Econolite.Ode.Model.SystemModeller.Db;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Test.Repository;
using Econolite.OdeRepository.SystemModeller;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace Econolite.Ode.Repository.SystemModeller.Test;

public class EntityModelEdgeRepositoryTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<EntityModelEdgeRepository> _logger = Mock.Of<ILogger<EntityModelEdgeRepository>>();
    private readonly IMongoContext _fixtureContext;
    private readonly EntityModelEdgeRepository _repo;

    public EntityModelEdgeRepositoryTests()
    {
        _memoryCache = new MemoryCacheMock();
        _repo = CreateRepository();
    }

    [Fact]
    public void LoadEntityNodesTest()
    {
        var result = _repo.GetAll();
    }

    [Fact]
    public async Task Intersection_GeoFence_Test()
    {
        var result = await _repo.QueryIntersectingGeoFences(new GeoJsonPoint() { Coordinates = new []{
            -83.04795026779175,
            42.536173595158324
            }});

        result.Should().HaveCount(1);
        result.Should().Contain(m => m.EntityType == "Intersection");
    }
    
    [Fact]
    public async Task IntersectionQuery_GeoFence_Test()
    {
        var result = await _repo.QueryIntersectingIntersections(new GeoJsonLineString(){
            Coordinates = new[]
            {
                new[] {-83.04776787757874, 42.53559352274027},
                new[] {-83.04779201745987, 42.53672994575431}
            }
        });

        result.Should().HaveCount(1);
        result.Should().Contain(m => m.EntityType == "Intersection");
    }
    
    [Fact]
    public async Task ApproachQuery_GeoFence_Test()
    {
        var result = await _repo.QueryIntersectingApproaches(new GeoJsonLineString(){
            Coordinates = new[]
            {
                new[] {-83.04776787757874, 42.53559352274027},
                new[] {-83.04779201745987, 42.53672994575431}
            }
        });

        result.Should().HaveCount(2);
        result.Should().Contain(m => m.EntityType == "Approach");
    }
    
    [Fact]
    public async Task StreetSegment_GeoFence_Test()
    {
        var result = await _repo.QueryIntersectingGeoFences(new GeoJsonPoint() { Coordinates = new []{
            -83.04737091064453,
            42.52612384452835
        }});

        result.Should().HaveCount(1);
        result.Should().Contain(m => m.EntityType == "StreetSegment");
    }
    
    [Fact]
    public async Task StreetSegmentQuery_Test()
    {
        var result = await _repo.QueryIntersectingStreetSegment(new GeoJsonPoint() { Coordinates = new []{
            -83.04737091064453,
            42.52612384452835
        }});

        result.Should().HaveCount(1);
        result.Should().Contain(m => m.EntityType == "StreetSegment");
    }

    protected Guid Id { get; } = Guid.NewGuid();
    
    protected EntityModelEdgeRepository CreateRepository()
    {
        return new EntityModelEdgeRepository(_fixtureContext, _memoryCache, _logger, "./data/config.json");
    }

    protected EntityModel CreateDocument()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
            
        var approach = new EntityModel()
        {
            EntityType = "Approach",
            Geometry = JsonSerializer.SerializeToDocument(new GeoJsonLineString(){
                Coordinates = new[]
                {
                    new[] {-111.89123779535294, 40.760442523425205},
                    new[] {-111.89096823334694, 40.76043642853944}
                },
                Type = "LineString"
            }, jsonOptions),
            Properties = JsonSerializer.SerializeToDocument(new ApproachPropertiesModel{Intersection = Guid.NewGuid().ToString(), Bearing = Bearing.NB.ToString(), Phases = new []{
                new PhaseModel(){ Number = 2, Movement = Movement.Thru.ToString(), Lanes = 2, Detectors = new []{new DetectorModel(){Advanced = true}}},
                new PhaseModel(){ Number = 1, Movement = Movement.Left.ToString(), Lanes = 1}}},jsonOptions)
        };

        return approach;
    }
}

public class MemoryCacheMock : IMemoryCache
{
    public void Dispose()
    {
    }

    public bool TryGetValue(object key, out object value)
    {
        value = LoadData();
        return true;
    }

    public ICacheEntry CreateEntry(object key)
    {
        throw new NotImplementedException();
    }

    public void Remove(object key)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<EntityModel>> GetOrCreateAsync(object key, Func<ICacheEntry, Task<IEnumerable<EntityModel>>> factory)
    {
        return await LoadDataAsync();
    }
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    
    private async Task<IEnumerable<EntityModel>> LoadDataAsync()
    {
        IEnumerable<EntityModel>? result = null;
        using StreamReader r = new StreamReader("./data/config.json");
        var json = await r.ReadToEndAsync();
        result = JsonSerializer.Deserialize<IEnumerable<EntityModel>>(json, _jsonSerializerOptions);
        return result ?? new List<EntityModel>();
    }
    
    private IEnumerable<EntityModel> LoadData()
    {
        IEnumerable<EntityModel>? result = null;
        using StreamReader r = new StreamReader("./data/config.json");
        var json = r.ReadToEnd();
        result = JsonSerializer.Deserialize<IEnumerable<EntityModel>>(json, _jsonSerializerOptions);
        return result ?? new List<EntityModel>();
    }
}