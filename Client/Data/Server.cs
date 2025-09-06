namespace Client.Data;

internal record struct Server(string Host, int Port)
{
    public static implicit operator (string host, int port)(Server value)
    {
        return (value.Host, value.Port);
    }

    public static implicit operator Server((string host, int port) value)
    {
        return new Server(value.host, value.port);
    }
}