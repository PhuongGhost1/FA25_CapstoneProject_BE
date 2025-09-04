using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using CusomMapOSM_Application.Common.ServiceConstants;

namespace CusomMapOSM_Infrastructure.Services;

public class RabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private readonly ConnectionFactory _factory;

    public RabbitMqService(ILogger<RabbitMqService> logger)
    {
        _logger = logger;

        _factory = new ConnectionFactory
        {
            HostName = RabbitMQConstant.Host,
            Port = RabbitMQConstant.Port,
            UserName = RabbitMQConstant.Username,
            Password = RabbitMQConstant.Password,
            VirtualHost = RabbitMQConstant.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
    }

    public IConnection CreateConnection()
    {
        _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port}", _factory.HostName, _factory.Port);
        return _factory.CreateConnection();
    }

    public IModel CreateEmailChannel(IConnection connection)
    {
        var channel = connection.CreateModel();

        channel.ExchangeDeclare(RabbitMQConstant.EmailExchange, ExchangeType.Direct, durable: true);

        channel.QueueDeclare(RabbitMQConstant.EmailDeadLetterQueue, durable: true, exclusive: false, autoDelete: false);

        var args = new Dictionary<string, object>
        {
            {"x-dead-letter-exchange", ""},
            {"x-dead-letter-routing-key", RabbitMQConstant.EmailDeadLetterQueue}
        };

        channel.QueueDeclare(RabbitMQConstant.EmailQueue, durable: true, exclusive: false, autoDelete: false, arguments: args);
        channel.QueueBind(RabbitMQConstant.EmailQueue, RabbitMQConstant.EmailExchange, routingKey: "email");

        _logger.LogInformation("Declared RabbitMQ email queues and exchange");
        return channel;
    }
}