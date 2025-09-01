using Client;

TCPClient client = new("127.0.0.1", 6665);
await client.RunAsync();