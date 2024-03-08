// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.SystemModeller;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Model.SystemModeller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Domain.Configuration;

public class SystemModellerConfigWorker: BackgroundService
{
    private const string EntityModelConfigRequestType = nameof(EntityModelConfigRequest);
    private readonly IConsumer<Guid, EntityModelConfigRequest> _consumer;
    private readonly ISystemModelerService _systemModelerService;
    private readonly ILogger<SystemModellerConfigWorker> _logger;

    public SystemModellerConfigWorker(IConfiguration configuration, IConsumer<Guid, EntityModelConfigRequest> consumer, IServiceProvider serviceProvider, ILogger<SystemModellerConfigWorker> logger)
    {
        _consumer = consumer;
        _systemModelerService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ISystemModelerService>();
        _logger = logger;
        var topic = configuration["Topics:ConfigPriorityRequest"];
        _consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed topic {@}", topic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);

                try
                {
                    var task = result.Type switch
                    {
                        EntityModelConfigRequestType => EntityModelConfigRequest(result),
                        _ => Task.CompletedTask
                    };

                    await task;
                    _consumer.Complete(result);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unhandled exception while processing: {@MessageType}", result.Type);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("System modeller config worker stopping");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception thrown while processing priority config request messages");
        }
    }

    private async Task EntityModelConfigRequest(ConsumeResult<Guid, EntityModelConfigRequest> result)
    {
        if (!result.DeviceId.HasValue)
        {
            return;
        }
        
        await _systemModelerService.PublishConfig(result.DeviceId.GetValueOrDefault());
    }
}