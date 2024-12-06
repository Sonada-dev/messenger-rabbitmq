using System.Text;
using RabbitMQ.Client.Events;

namespace Messenger.Infrastructure.RabbitMq;

public class RabbitMqConsumer(RabbitMqConnectionManager connectionManager)
{
    public async Task StartConsumingAsync(
        string queueName,
        Func<string, Task> onMessageReceived,
        string consumerTag = "",
        bool autoAck = true,
        bool noLocal = false,
        bool exclusive = false,
        IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        // Создаем асинхронный канал
        var connection = await connectionManager.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        try
        {
            // Убедимся, что очередь существует
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);

            // Создаем потребителя
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Вызываем переданную функцию для обработки сообщения
                await onMessageReceived(message);
            };

            // Начинаем потребление
            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: autoAck,
                consumerTag: consumerTag,
                noLocal: noLocal,
                exclusive: exclusive,
                arguments: arguments,
                consumer: consumer,
                cancellationToken: cancellationToken
            );

            // Держим потребление активным до отмены
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            // Закрываем канал с кодом завершения 200 (Normal Shutdown)
            await channel.CloseAsync(200, "Normal Shutdown", false, cancellationToken);
        }
    }
}