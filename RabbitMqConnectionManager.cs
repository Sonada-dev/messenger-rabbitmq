using RabbitMQ.Client;

namespace Messenger.Infrastructure.RabbitMq;

public class RabbitMqConnectionManager(RabbitMqConfiguration config)
{
    private readonly ConnectionFactory _factory = new()
    {
        HostName = config.HostName,
        UserName = config.UserName,
        Password = config.Password,
        VirtualHost = config.VirtualHost,
        Port = config.Port
    };

    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is null || !_connection.IsOpen)
        {
            _connection = await _factory.CreateConnectionAsync();
        }
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
