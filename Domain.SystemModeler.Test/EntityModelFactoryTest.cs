// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System;
using System.Text.Json;
using Econolite.Ode.Model.SystemModeller;
using Econolite.Ode.Model.SystemModeller.Db;
using Xunit;

namespace Econolite.Ode.Domain.SystemModeller.Test;

public class EntityModelFactoryTest
{
    [Fact]
    public void CreateStreetSegment_FromStreetSegmentWithOutTripPoints_ResultWithTripPoints()
    {
        var streetSegment = new EntityModel()
        {
            EntityType = "StreetSegment",
            Geometry = JsonSerializer.SerializeToDocument(new GeoJsonLineString(){
                Coordinates = new[]
                {
                    new[] {-84.190029501915, 34.017980435211},
                    new[] {-84.1900938749313, 34.0177892414078},
                    new[] {-84.190571308136, 34.0163397115892},
                    new[] {-84.1910004615784, 34.0149657479003}
                },
                Type = "LineString"
            }),
            Properties = JsonSerializer.SerializeToDocument(new StreetSegmentPropertiesModel{Origin = Guid.NewGuid().ToString(), Destination = Guid.NewGuid().ToString(), SpeedLimit = 35 })
        };
        
        var result = EntityModelFactory.Create(streetSegment);
    }
}