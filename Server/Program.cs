using Server;

AsyncTCPServer server = new(6665);

// Run the async server loop
_ = Task.Run(() => server.RunAsync());
bool running = true;

while (running)
{
    var cmd = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(cmd))
        continue;

    var parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLower();
    var arg = parts.Length > 1 ? parts[1] : null;

    switch (command)
    {
        case "stop":
            server.Shutdown();
            running = false;
            break;

        case "op":
            if (arg == null) { Console.WriteLine("Usage: op <username>"); break; }
            server.AddGlobalOp(arg);
            Console.WriteLine($"{arg} is now a server operator.");
            break;

        case "rmchan":
            if (arg == null) { Console.WriteLine("Usage: rmchan <#channel>"); break; }
            server.RemoveChannel(arg, "console");
            break;

        case "kick":
            if (arg == null) { Console.WriteLine("Usage: kick <username>"); break; }
            if (server.KickUser(arg))
                Console.WriteLine($"{arg} was kicked.");
            else
                Console.WriteLine($"User {arg} not found.");
            break;

        case "ban":
            if (arg == null) { Console.WriteLine("Usage: ban <ip>"); break; }
            if (server.BanIp(arg))
                Console.WriteLine($"{arg} was banned.");
            else
                Console.WriteLine($"Could not ban {arg}.");
            break;

        case "unban":
            if (arg == null) { Console.WriteLine("Usage: unban <ip>"); break; }
            if (server.UnbanIp(arg))
                Console.WriteLine($"{arg} was unbanned.");
            else
                Console.WriteLine($"IP {arg} was not banned.");
            break;

        case "listops":
            var ops = server.GetGlobalOps();
            if (ops.Count == 0) Console.WriteLine("No global operators.");
            else Console.WriteLine("Global operators: " + string.Join(", ", ops));
            break;

        default:
            Console.WriteLine("Unknown Command");
            break;
    }
}
