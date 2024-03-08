// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Model.SystemModeller;
using Econolite.OdeRepository.SystemModeller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Domain.Configuration;

public class SystemModellerEdgeConfigWorker : BackgroundService
{
    private const string EntityModelJsonConfigResponse = nameof(EntityModelJsonConfigResponse);
    private readonly IEntityModelJsonFileRepository _entityModelJsonFileRepository;
    private readonly IConsumer<Guid, GenericJsonResponse> _consumer;
    private readonly IProducer<Guid, EntityModelConfigRequest> _producer;
    private readonly MessageFactory<Guid, EntityModelConfigRequest> _messageFactory;
    private readonly ILogger<SystemModellerEdgeConfigWorker> _logger;
    private readonly string _requestTopic;
    private readonly string _topic;
    private readonly string _intersection;

    public SystemModellerEdgeConfigWorker(IConfiguration configuration, IEntityModelJsonFileRepository entityModelJsonFileRepository, IConsumer<Guid, GenericJsonResponse> consumer, IProducer<Guid, EntityModelConfigRequest> producer, MessageFactory<Guid, EntityModelConfigRequest> messageFactory, ILogger<SystemModellerEdgeConfigWorker> logger)
    {
        _entityModelJsonFileRepository = entityModelJsonFileRepository;
        _consumer = consumer;
        _producer = producer;
        _messageFactory = messageFactory;
        _logger = logger;
        _requestTopic = configuration["Topics:ConfigPriorityRequest"];
        _topic = configuration["Topics:ConfigPriorityResponse"];
        _intersection = configuration["Intersection"];
        consumer.Subscribe(TopicWithIntersectionId());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () => 
        {
            try
            {
                var id = Guid.Parse(_intersection);
                await _producer.ProduceAsync(_requestTopic, _messageFactory.Build(_ => id, id, new EntityModelConfigRequest()));
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);

                    try
                    {
                        var task = result.Type switch
                        {
                            EntityModelJsonConfigResponse => EntityModelConfigResponse(result),
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
        });
    }

    private async Task EntityModelConfigResponse(ConsumeResult<Guid, GenericJsonResponse> result)
    {
        if (!result.DeviceId.HasValue)
        {
            return;
        }

        var value = JsonSerializer.Deserialize<IEnumerable<EntityModel>>(result.Value.Json,
            JsonPayloadSerializerOptions.Options);
        await _entityModelJsonFileRepository.SaveJsonAsync(value);
    }
    
    private string TopicWithIntersectionId()
    {
        return $"{_topic}.{_intersection}";
    }
}