using System.Text;
using Newtonsoft.Json;
using BasicProperties = RabbitMQ.Client.BasicProperties;

namespace Messenger.Infrastructure.RabbitMq;

public class RabbitMqProducer(RabbitMqConnectionManager connectionManager)
{
    public async Task PublishAsync(string queueName, object message, CancellationToken cancellationToken = default)
    {
        // Создаем асинхронный канал
        var connection = await connectionManager.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        try
        {
            // Убедимся, что очередь существует
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);

            // Сериализуем сообщение в JSON
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            // Создаем базовые свойства
            var properties = new BasicProperties();

            // Публикуем сообщение
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body.AsMemory(),
                cancellationToken: cancellationToken
            );
        }
        finally
        {
            // Закрываем канал с кодом завершения 200 (Normal Shutdown)
            await channel.CloseAsync(200, "Normal Shutdown", false, cancellationToken);
        }
    }
}
