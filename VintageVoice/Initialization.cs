using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VintageVoice;

public class Initialization : ModSystem
{
    Client.Instance clientAPI = new();
    Server.Instance serverAPI = new();
    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        clientAPI.Init(api);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        serverAPI.Init(api);
    }
}

public class Debug
{
    private static readonly OperatingSystem system = Environment.OSVersion;
    static private ILogger loggerForNonTerminalUsers;

    static public void LoadLogger(ILogger logger) => loggerForNonTerminalUsers = logger;
    static public void Log(string message)
    {
        // Check if is linux or other based system and if the terminal is active for the logs to be show
        if ((system.Platform == PlatformID.Unix || system.Platform == PlatformID.Other) && Environment.UserInteractive)
            // Based terminal users
            Console.WriteLine($"{DateTime.Now:d.M.yyyy HH:mm:ss} [VintageVoice] {message}");
        else
            // Unbased non terminal users
            loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[VintageVoice] {message}");
    }
}