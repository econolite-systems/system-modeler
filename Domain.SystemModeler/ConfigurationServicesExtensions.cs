// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Model.SystemModeller;
using Econolite.OdeRepository.SystemModeller;
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Domain.SystemModeller;

public static class ConfigurationServicesExtensions
{
    public static IServiceCollection AddSystemModelerService(this IServiceCollection services)
    {
        // services.AddTransient(_ =>
        //     new ConsumeResultFactory<Guid, EntityModelConfigRequest>(Guid.Parse,
        //         (string type, string data) => Payload.ToObject<EntityModelConfigRequest>(data)));
        // services.AddTransient<IConsumer<Guid, EntityModelConfigRequest>, Consumer<Guid, EntityModelConfigRequest>>();
        services.AddSingleton(_ => new MessageFactory<Guid, GenericJsonResponse>((x) => new ToStringElement<Guid>(x), (x) => new BaseJsonPayload<GenericJsonResponse>(x)));
        services.AddTransient<IProducer<Guid, GenericJsonResponse>, Producer<Guid, GenericJsonResponse>>();
        services.AddScoped<ISystemModelerService, SystemModelerService>();
        // services.AddHostedService<SystemModellerConfigWorker>();
        return services;
    }
    
    public static IServiceCollection AddSystemModelerConfigChange(this IServiceCollection services)
    {
        services.AddTransient(_ =>
            new ConsumeResultFactory<Guid, EntityModelConfigRequest>(Guid.Parse,
                (string type, string data) => Payload.ToObject<EntityModelConfigRequest>(data)));
        services.AddTransient<IConsumer<Guid, EntityModelConfigRequest>, Consumer<Guid, EntityModelConfigRequest>>();
        // services.AddTransient(_ =>
        //     new ConsumeResultFactory<Guid, GenericJsonResponse>(Guid.Parse,
        //         (string type, string data) => new GenericJsonResponse(data)));
        // services.AddTransient<IConsumer<Guid, GenericJsonResponse>, Consumer<Guid, GenericJsonResponse>>();
        // services.AddSingleton(_ => new MessageFactory<Guid, EntityModelConfigResponse>((x) => new ToStringElement<Guid>(x), (x) => new BaseJsonPayload<EntityModelConfigResponse>(x)));
        // services.AddTransient<IProducer<Guid, EntityModelConfigResponse>, Producer<Guid, EntityModelConfigResponse>>();
        services.AddHostedService<SystemModellerConfigWorker>();
        return services;
    }
    
    public static IServiceCollection AddSystemModellerEdgeService(this IServiceCollection services)
    {
        services.AddTransient(_ =>
            new ConsumeResultFactory<Guid, GenericJsonResponse>(Guid.Parse,
                (string type, string data) => Payload.ToObject<GenericJsonResponse>(data)));
        services.AddTransient<IConsumer<Guid, GenericJsonResponse>, Consumer<Guid, GenericJsonResponse>>();
        services.AddSingleton(_ => new MessageFactory<Guid, EntityModelConfigRequest>((x) => new ToStringElement<Guid>(x), (x) => new BaseJsonPayload<EntityModelConfigRequest>(x)));
        services.AddTransient<IProducer<Guid, EntityModelConfigRequest>, Producer<Guid, EntityModelConfigRequest>>();
        services.AddScoped<ISystemModelerService, SystemModelerEdgeService>();
        services.AddSingleton<IEntityModelRepository, EntityModelEdgeRepository>();
        services.AddHostedService<SystemModellerEdgeConfigWorker>();
        return services;
    }
}