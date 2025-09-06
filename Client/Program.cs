using Client;


LocalClient.RegisterCommands();
LocalClient.LoadServers();

await LocalClient.RunAsync();

LocalClient.SaveServers();


