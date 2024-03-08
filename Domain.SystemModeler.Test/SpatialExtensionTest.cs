// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Econolite.Ode.Model.SystemModeller;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Xunit;

namespace Econolite.Ode.Domain.SystemModeller.Test;

public class SpatialExtensionTest
{
    [Fact]
    public void CoordinateFromDistanceAndHeading()
    {
        var start = (-84.190029501915, 34.017980435211).ToCoordinate();
        var end = (-84.1910004615784, 34.0149657479003).ToCoordinate();
        var distance = start.DistanceTo(end);
        var heading = start.HeadingTo(end);
        var result = start.ToCoordinate(distance,heading);

        result.Should().Be(end);
    }
    
    [Fact]
    public void Generate_StreetSegment_TripPointLocations()
    {
        LineString lineString = ((IEnumerable<Coordinate>) new Coordinate[]
        {
            (-84.190029501915, 34.017980435211).ToCoordinate(),
            (-84.1900938749313, 34.0177892414078).ToCoordinate(),
            (-84.190571308136, 34.0163397115892).ToCoordinate(),
            (-84.1910004615784, 34.0149657479003).ToCoordinate()
        }).ToLineString(4326);

        var lengthInMeters = lineString.ToLength();
        var coordinate = lineString.ToCoordinate(15);
        var coordinate2 = lineString.ToCoordinate(45);
        var coordinate3 = lineString.ToCoordinate(300);
        var locations = lineString.ToTripPointLocations(15);
    }
    
    [Fact]
    public void GenerateStreetSegment_FromGeoJsonGeometry_TripPointLocations()
    {
        var geoJsonGeometry  = new GeoJsonLineString()
        {
            Type = "LineString",
            Coordinates = new[]
                    {
                        new[] {-84.190029501915, 34.017980435211},
                        new[] {-84.1900938749313, 34.0177892414078},
                        new[] {-84.190571308136, 34.0163397115892},
                        new[] {-84.1910004615784, 34.0149657479003}
                    }
        };

        // var lengthInMeters = lineString.ToLength();
        // var coordinate = lineString.ToCoordinate(15);
        // var coordinate2 = lineString.ToCoordinate(45);
        // var coordinate3 = lineString.ToCoordinate(300);
        var locations = geoJsonGeometry.ToTripPointLocations(15);
    }
    
    [Fact]
    public void TripPointLocations()
    {
        var geoJsonGeometry  = new GeoJsonLineString()
        {
            Type = "LineString",
            Coordinates = new[]
            {
                new[] {-84.190029501915, 34.017980435211},
                new[] {-84.1900938749313, 34.0177892414078},
                new[] {-84.190571308136, 34.0163397115892},
                new[] {-84.1910004615784, 34.0149657479003}
            }
        };

        // var lengthInMeters = lineString.ToLength();
        // var coordinate = lineString.ToCoordinate(15);
        // var coordinate2 = lineString.ToCoordinate(45);
        // var coordinate3 = lineString.ToCoordinate(300);
        var locations = geoJsonGeometry.ToTripPointLocations(15);
    }
    
    [Fact]
    public void TripPointLocationForIntersections()
    {
        var geoJsonGeometry  = new GeoJsonLineString()
        {
            Type = "LineString",
            Coordinates = new[]
            {
                new[] {
                -83.04657697677612,
                42.506179656801315
                },
                new[] {
                -83.04674863815308,
                42.51380380123564
                },
                new[] {
                -83.04694175720215,
                42.51742573485527
                },
                new[] {
                -83.0472207069397,
                42.51970317513057
                },
                new[] {
                -83.04730653762817,
                42.52036741290862
                },
                new[] {
                -83.04741382598877,
                42.52277126203762
                },
                new[] {
                -83.04782152175902,
                42.531389566042044
                },
                new[] {
                -83.04799318313599,
                42.535737800199456
                },
                new[] {
                -83.04797172546387,
                42.53822011085412
                },
                new[] {
                -83.04790735244751,
                42.53896320376963
                },
                new[] {
                -83.04825067520142,
                42.546757241268104
                },
                new[] {
                -83.0484652519226,
                42.55132570743411
                },
                new[] {
                -83.0491518974304,
                42.56235820171796
                }
            }
        };
        var locations = geoJsonGeometry.ToTripPointLocations(50, false);
        var intersections = new[]
        {
            new GeoJsonPoint()
            {
                Coordinates = new []
                {
                    -83.04795026779175,
                    42.536173595158324
                }
            },
            new GeoJsonPoint()
            {
                Coordinates = new []
                {
                    -83.04835125803947,
                    42.550627220095365
                }
            },
            new GeoJsonPoint()
            {
                Coordinates = new []
                {
                    -83.04728910326958,
                    42.520786511682985
                }
            },
        };

        var orderedBy = intersections.Select(i => (i.ToTripPointLocation(locations), i)).OrderBy(l => l.Item1.Distance);
    }

    // Grouping<string, ValueTuple<TripPointLocation,EntityModel>>
    public EntityModel[] NbSbApproaches()
    {
        return new [] {
            CreateApproach("d87e6b14-20e8-4b5a-9662-491735a0b785", "4991aa1a-30f0-4b63-ad9d-713309b0b519", "SB", 4, new[]
            {
                new[]
                {
                    -83.04827481508255,
                    42.53637419816486
                },
                new[]
                {
                    -83.04765924811363,
                    42.53638012730797
                }
            }),
            CreateApproach("dfe9c4b9-e91b-4791-b428-89a08be407ea", "4991aa1a-30f0-4b63-ad9d-713309b0b519", "NB", 8, new[]
            {
                new[]
                {
                    -83.04764449596405,
                    42.5359927554425
                },
                new[]
                {
                    -83.04827615618706,
                    42.535987814459304
                }
            }),
            CreateApproach("61ba9237-1c2e-4c7c-a78f-bc9ee2ca0151", "96be1357-1371-42a6-aea3-7129713c24dd", "SB", 4, new[]
            {
                new[]
                {
                    -83.04872408509254,
                    42.550800607710066
                },
                new[]
                {
                    -83.04805621504784,
                    42.55082234287567
                }
            }),
            CreateApproach("eb3992a3-15c3-40df-9b52-d62fa57c2267", "96be1357-1371-42a6-aea3-7129713c24dd", "NB", 8, new[]
            {
                new[]
                {
                    -83.047995865345,
                    42.55043506060925
                },
                new[]
                {
                    -83.0486610531807,
                    42.55041678319802
                }
            }),
            CreateApproach("e6bee362-2ebe-470c-b94c-5ed7b5902d0b", "bc8190fc-ab3f-4cdd-8eae-8f8ff1641eb6", "SB", 4, new[]
            {
                new[]
                {
                    -83.0476726591587,
                    42.52102175929149
                },
                new[]
                {
                    -83.04697662591933,
                    42.52109094959601
                }
            }),
            CreateApproach("a4350efd-ee81-4b93-a144-7d650b4dc212", "bc8190fc-ab3f-4cdd-8eae-8f8ff1641eb6", "NB", 8, new[]
            {
                new[]
                {
                    -83.04694309830666,
                    42.520578939528164
                },
                new[]
                {
                    -83.04762974381447,
                    42.52055917071556
                }
            })
        };
    }
    
    public EntityModel[] AllApproaches()
    {
        return new [] {
            CreateApproach("d87e6b14-20e8-4b5a-9662-491735a0b785", "4991aa1a-30f0-4b63-ad9d-713309b0b519", "SB", 4, new[]
            {
                new[]
                {
                    -83.04827481508255,
                    42.53637419816486
                },
                new[]
                {
                    -83.04765924811363,
                    42.53638012730797
                }
            }),
            CreateApproach("dfe9c4b9-e91b-4791-b428-89a08be407ea", "4991aa1a-30f0-4b63-ad9d-713309b0b519", "NB", 8, new[]
            {
                new[]
                {
                    -83.04764449596405,
                    42.5359927554425
                },
                new[]
                {
                    -83.04827615618706,
                    42.535987814459304
                }
            }),
            CreateApproach("7cf6e6df-21c2-4159-b5cb-f0fe9f4683d3", "4991aa1a-30f0-4b63-ad9d-713309b0b519", "WB", 2, new[]
            {
                new[]
                {
                    -83.04754257202148,
                    42.536265497108296
                },
                new[]
                {
                    -83.04753988981246,
                    42.536106397948025
                }
            }),
            CreateApproach("c647623d-8b37-4398-9213-72d231ece831", "4991aa1a-30f0-4b63-ad9d-713309b0b519", "EB", 6, new[]
            {
                new[]
                {
                    -83.04836802184582,
                    42.53604858852541
                },
                new[]
                {
                    -83.04837539792061,
                    42.53624079229632
                }
            }),
            CreateApproach("61ba9237-1c2e-4c7c-a78f-bc9ee2ca0151", "96be1357-1371-42a6-aea3-7129713c24dd", "SB", 4, new[]
            {
                new[]
                {
                    -83.04872408509254,
                    42.550800607710066
                },
                new[]
                {
                    -83.04805621504784,
                    42.55082234287567
                }
            }),
            CreateApproach("eb3992a3-15c3-40df-9b52-d62fa57c2267", "96be1357-1371-42a6-aea3-7129713c24dd", "NB", 8, new[]
            {
                new[]
                {
                    -83.047995865345,
                    42.55043506060925
                },
                new[]
                {
                    -83.0486610531807,
                    42.55041678319802
                }
            }),
            CreateApproach("ebb8ee71-64f7-463e-9756-6da87e350672", "96be1357-1371-42a6-aea3-7129713c24dd", "WB", 2, new[]
            {
                new[]
                {
                    -83.0479408800602,
                    42.55074577578143
                },
                new[]
                {
                    -83.04792076349258,
                    42.55053879716625
                }
            }),
            CreateApproach("f2120cf6-e09a-42f7-9df1-564d9025ec22", "96be1357-1371-42a6-aea3-7129713c24dd", "EB", 6, new[]
            {
                new[]
                {
                    -83.04880656301975,
                    42.550500760448706
                },
                new[]
                {
                    -83.0488159507513,
                    42.55069341371451
                }
            }),
            CreateApproach("e6bee362-2ebe-470c-b94c-5ed7b5902d0b", "bc8190fc-ab3f-4cdd-8eae-8f8ff1641eb6", "SB", 4, new[]
            {
                new[]
                {
                    -83.0476726591587,
                    42.52102175929149
                },
                new[]
                {
                    -83.04697662591933,
                    42.52109094959601
                }
            }),
            CreateApproach("a4350efd-ee81-4b93-a144-7d650b4dc212", "bc8190fc-ab3f-4cdd-8eae-8f8ff1641eb6", "NB", 8, new[]
            {
                new[]
                {
                    -83.04694309830666,
                    42.520578939528164
                },
                new[]
                {
                    -83.04762974381447,
                    42.52055917071556
                }
            }),
            CreateApproach("fed67bd1-d351-4ec4-bffb-0559771b3801", "bc8190fc-ab3f-4cdd-8eae-8f8ff1641eb6", "WB", 2, new[]
            {
                new[]
                {
                    -83.04688140749931,
                    42.52104943542249
                },
                new[]
                {
                    -83.04687470197676,
                    42.520726216985274
                }
            }),
            CreateApproach("90921678-35ee-47d0-a278-457eb3ca9b80", "bc8190fc-ab3f-4cdd-8eae-8f8ff1641eb6", "EB", 6, new[]
            {
                new[]
                {
                    -83.0477826297283,
                    42.52060958117537
                },
                new[]
                {
                    -83.0477799475193,
                    42.520910066209645
                }
            })
        };
    }
    
    public EntityModel CreateApproach(string id, string intersection, string bearing, int phase, double[][] coordinates, string movement = "Thru")
    {
        return new EntityModel()
        {
            Id = Guid.Parse(id),
            EntityType = "Approach",
            Geometry = JsonSerializer.SerializeToDocument(new GeoJsonLineString(){
                Coordinates = coordinates,
                Type = "LineString"
            }),
            Properties = JsonSerializer.SerializeToDocument(new ApproachPropertiesModel()
            {
                Intersection = intersection,
                Bearing = bearing,
                SpeedLimit = 55,
                Phases = new []{
                    new PhaseModel()
                    {
                        Number = phase,
                        Movement = movement,
                        Lanes = 4
                    }
                }
            })
        };
    }
}