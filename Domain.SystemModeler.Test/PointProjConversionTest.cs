// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Model.SystemModeller.Db;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

namespace Econolite.Ode.Domain.SystemModeller.Test;

public class PointProjConversionTest
{
  [Fact]
  public void GeometryFromSphericalMercatorToLonLatTest()
  {
    LineString lineString = ((IEnumerable<Coordinate>) new Coordinate[4]
    {
      (-84.190029501915, 34.017980435211).ToCoordinate(),
      (-84.1900938749313, 34.0177892414078).ToCoordinate(),
      (-84.190571308136, 34.0163397115892).ToCoordinate(),
      (-84.1910004615784, 34.0149657479003).ToCoordinate()
    }).ToLineString(4326);
    lineString.CreateBuffer(0.0001).ToGeoJson();
    (lineString.ProjectTo(EpsgCode.WebMercator) as LineString ?? throw new InvalidOperationException()).CreateBuffer(2286.0 / 625.0).ToLatLon().ToGeoJson();
  }

  [Fact]
  public void SphericalMercatorToLonLatTest()
  {
    Coordinate latLon = (-9372100.7061, 4030811.38939).ToLatLon();
    Assert.Equal<double>(-84.191013349968642, latLon.X);
    Assert.Equal<double>(34.014963249984042, latLon.Y);
  }

  [Fact]
  public void LonLatToSphericalMercatorTest()
  {
    Coordinate sphericalMercator = (-84.1910133499686, 34.014963249984).ToSphericalMercator();
    Assert.Equal<double>(-9372100.7060999945, sphericalMercator.X);
    Assert.Equal<double>(4030811.3893899922, sphericalMercator.Y);
  }

  [Fact]
  public void LonLat4326To2926Test()
  {
    double num1 = -84.191013;
    double num2 = 34.014963;
    Coordinate coordinate = (num1, num2).ToCoordinate();
    (num1, num2).ToSphericalMercator();
    coordinate.ProjectTo(EpsgCode.Wgs84, EpsgCode.WebMercator).ProjectTo(EpsgCode.WebMercator, EpsgCode.Wgs84);
  }

  [Fact]
  public void CreateGeometryPointTest()
  {
    Coordinate coordinate = (-9372100.7061, 4030811.28939).ToCoordinate();
    Geometry.DefaultFactory.CreatePoint(coordinate).SRID = 3857;
  }

  [Fact]
  public void CreateGeometryLineStringTest()
  {
    Coordinate[] coordinates = new Coordinate[2]
    {
      new Coordinate(-9372100.7061, 4030811.28939),
      new Coordinate(-9372100.7061, 4030911.28939)
    };
    LineString lineString = Geometry.DefaultFactory.CreateLineString(coordinates);
    lineString.SRID = 3857;
    double length = lineString.Length;
    AngleUtility.ToDegrees(AngleUtility.Angle(lineString.StartPoint.Coordinate, lineString.EndPoint.Coordinate));
  }

  [Fact]
  public void GetNorthBoundTest()
  {
    Coordinate coordinate = new Coordinate(-9372100.7061, 4030811.28939);
    Coordinate northBound = coordinate.ToNorthBound();
    Assert.Equal<double>(-90.0, AngleUtility.ToDegrees(AngleUtility.Angle(coordinate, northBound)));
  }

  [Fact]
  public void GetEastBoundTest()
  {
    Coordinate coordinate = new Coordinate(-9372100.7061, 4030811.28939);
    Coordinate eastBound = coordinate.ToEastBound();
    Assert.Equal<double>(180.0, AngleUtility.ToDegrees(AngleUtility.Angle(coordinate, eastBound)));
  }

  [Fact]
  public void GetSouthBoundTest()
  {
    Coordinate coordinate = new Coordinate(-9372100.7061, 4030811.28939);
    Coordinate southBound = coordinate.ToSouthBound();
    Assert.Equal<double>(90.0, AngleUtility.ToDegrees(AngleUtility.Angle(coordinate, southBound)));
  }

  [Fact]
  public void GetWestBoundTest()
  {
    Coordinate coordinate = new Coordinate(-9372100.7061, 4030811.28939);
    Coordinate westBound = coordinate.ToWestBound();
    Assert.Equal<double>(0.0, AngleUtility.ToDegrees(AngleUtility.Angle(coordinate, westBound)));
  }

  [Fact]
  public void GetLineStringNorthBoundOnLowerTest()
  {
    LineString stringFromDirection = (9372100.7061, -4030811.28939).ToCoordinate().ToLineStringFromDirection("NB");
    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
    interpolatedStringHandler.AppendLiteral("https://epsg.io/map#srs=3857&x=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.X);
    interpolatedStringHandler.AppendLiteral("&y=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.Y);
    interpolatedStringHandler.AppendLiteral("&z=20&layer=streets");
    Console.Out.WriteLine(interpolatedStringHandler.ToStringAndClear());
    double degrees = AngleUtility.ToDegrees(AngleUtility.Angle(stringFromDirection.StartPoint.Coordinate, stringFromDirection.EndPoint.Coordinate));
    (Bearing Bearing, string? Error) bearing = stringFromDirection.GetBearing(true);
    Assert.Equal<double>(-90.0, degrees);
    Assert.Equal<Bearing>(Bearing.NB, bearing.Bearing);
  }

  [Fact]
  public void GetLineStringNorthBoundTest()
  {
    LineString stringFromDirection = (-9372100.7061, 4030811.28939).ToCoordinate().ToLineStringFromDirection("NB");
    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
    interpolatedStringHandler.AppendLiteral("https://epsg.io/map#srs=3857&x=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.X);
    interpolatedStringHandler.AppendLiteral("&y=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.Y);
    interpolatedStringHandler.AppendLiteral("&z=20&layer=streets");
    Console.Out.WriteLine(interpolatedStringHandler.ToStringAndClear());
    double degrees = AngleUtility.ToDegrees(AngleUtility.Angle(stringFromDirection.StartPoint.Coordinate, stringFromDirection.EndPoint.Coordinate));
    (Bearing Bearing, string? Error) bearing = stringFromDirection.GetBearing(true);
    Assert.Equal<double>(-90.0, degrees);
    Assert.Equal<Bearing>(Bearing.NB, bearing.Bearing);
  }

  [Fact]
  public void GetLineStringEastBoundTest()
  {
    string bearing1 = "EB";
    LineString stringFromDirection = (-9372100.7061, 4030811.28939).ToCoordinate().ToLineStringFromDirection(bearing1);
    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
    interpolatedStringHandler.AppendLiteral("https://epsg.io/map#srs=3857&x=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.X);
    interpolatedStringHandler.AppendLiteral("&y=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.Y);
    interpolatedStringHandler.AppendLiteral("&z=20&layer=streets");
    Console.Out.WriteLine(interpolatedStringHandler.ToStringAndClear());
    double degrees = AngleUtility.ToDegrees(AngleUtility.Angle(stringFromDirection.StartPoint.Coordinate, stringFromDirection.EndPoint.Coordinate));
    (Bearing Bearing, string? Error) bearing2 = stringFromDirection.GetBearing(true);
    Assert.Equal<double>(180.0, degrees);
    Assert.Equal<Bearing>(Bearing.EB, bearing2.Bearing);
  }

  [Fact]
  public void GetLineStringSouthBoundTest()
  {
    string bearing1 = "SB";
    LineString stringFromDirection = (-9372100.7061, 4030811.28939).ToCoordinate().ToLineStringFromDirection(bearing1);
    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
    interpolatedStringHandler.AppendLiteral("https://epsg.io/map#srs=3857&x=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.X);
    interpolatedStringHandler.AppendLiteral("&y=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.Y);
    interpolatedStringHandler.AppendLiteral("&z=20&layer=streets");
    Console.Out.WriteLine(interpolatedStringHandler.ToStringAndClear());
    double degrees = AngleUtility.ToDegrees(AngleUtility.Angle(stringFromDirection.StartPoint.Coordinate, stringFromDirection.EndPoint.Coordinate));
    (Bearing Bearing, string? Error) bearing2 = stringFromDirection.GetBearing(true);
    Assert.Equal<double>(90.0, degrees);
    Assert.Equal<Bearing>(Bearing.SB, bearing2.Bearing);
  }

  [Fact]
  public void GetLineStringWestBoundTest()
  {
    string bearing1 = "WB";
    LineString stringFromDirection = (-9372100.7061, 4030811.28939).ToCoordinate().ToLineStringFromDirection(bearing1);
    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
    interpolatedStringHandler.AppendLiteral("https://epsg.io/map#srs=3857&x=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.X);
    interpolatedStringHandler.AppendLiteral("&y=");
    interpolatedStringHandler.AppendFormatted<double>(stringFromDirection.EndPoint.Y);
    interpolatedStringHandler.AppendLiteral("&z=20&layer=streets");
    Console.Out.WriteLine(interpolatedStringHandler.ToStringAndClear());
    double degrees = AngleUtility.ToDegrees(AngleUtility.Angle(stringFromDirection.StartPoint.Coordinate, stringFromDirection.EndPoint.Coordinate));
    (Bearing Bearing, string? Error) bearing2 = stringFromDirection.GetBearing(true);
    Assert.Equal<double>(0.0, degrees);
    Assert.Equal<Bearing>(Bearing.WB, bearing2.Bearing);
  }

  [Fact]
  public void DistanceForLinestringTest()
  {
    LineString lineString = ((IEnumerable<Coordinate>) new Coordinate[]
    {
      (-84.190029501915, 34.017980435211).ToCoordinate(),
      (-84.1900938749313, 34.0177892414078).ToCoordinate(),
      (-84.190571308136, 34.0163397115892).ToCoordinate(),
      (-84.1910004615784, 34.0149657479003).ToCoordinate()
    }).ToLineString(4326);
    var buffer = lineString.CreateBuffer(0.0001).ToGeoJson();
    Point point1 = Geometry.DefaultFactory.WithSRID(EpsgCode.Wgs84.ToInt()).CreatePoint((-84.190029501915, 34.017980435211).ToCoordinate()).ProjectTo(EpsgCode.WebMercator) as Point ?? throw new InvalidOperationException();
    Point point2 = Geometry.DefaultFactory.WithSRID(EpsgCode.Wgs84.ToInt()).CreatePoint((-84.1910004615784, 34.0149657479003).ToCoordinate()).ProjectTo(EpsgCode.WebMercator) as Point ?? throw new InvalidOperationException();
    var distance = point1.Distance(point2);


    var gc1 = (-84.190029501915, 34.017980435211).ToGeoCoordinate();
    var gc2 = (-84.1910004615784, 34.0149657479003).ToGeoCoordinate();
    var gc3 = (-84.190029501915, 34.017980435211).ToCoordinate();
    var gc4 = (-84.1910004615784, 34.0149657479003).ToCoordinate();
    var gc1Distance = gc1.GetDistanceTo(gc2);
    var gc2Distance = gc3.DistanceTo(gc4);
    var angle = gc3.HeadingTo(gc4);
    var gc5 = gc3.ToCoordinate(gc2Distance,angle);
  }
}