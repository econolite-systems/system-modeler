// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Domain.SystemModeller;
using Econolite.Ode.Model.SystemModeller;
using Econolite.Ode.Modules;
using Econolite.OdeRepository.SystemModeller;

namespace Module.SystemModeler.Modules;

public class ConfigModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection builder)
    {
        builder.AddSystemModelerService();
        builder.AddSystemModellerRepo();
        builder.AddSystemModelerConfigChange();
        return builder;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("system-modeler", async (ISystemModelerService service) =>
        {
            var result = await service.GetAllAsync();
            return Results.Ok(result);
        }).Produces<IEnumerable<EntityModel>>();
        
        app.MapGet("system-modeler/feature/{id}", async (ISystemModelerService service, Guid id) =>
        {
            var result = await service.GetByIdAsync(id);
            return Results.Ok(result);
        }).Produces<EntityModel>();
        
        return app;
    }
} 