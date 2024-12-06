namespace Messenger.Infrastructure.RabbitMq;

public class RabbitMqConfiguration
{
    public string HostName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string VirtualHost { get; set; } = "/";
    public int Port { get; set; } = 5672; // Стандартный порт
}
