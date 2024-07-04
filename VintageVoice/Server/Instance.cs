using System;
using Vintagestory.API.Server;

namespace VintageVoice.Server;
public class Instance
{
    ICoreServerAPI serverAPI;
    public IServerNetworkChannel communicationChannel;
    private readonly VoiceSender voiceSender = new();

    public void Init(ICoreServerAPI api)
    {
        serverAPI = api;
        voiceSender.Init(serverAPI.World);
        communicationChannel = serverAPI.Network.RegisterChannel("VintageVoice_VoiceBuffer").RegisterMessageType(typeof(byte[]));
        communicationChannel.SetMessageHandler<byte[]>(voiceSender.OnCommunicationReceived);
        Debug.Log("Channel instanciated");
        serverAPI.Event.PlayerJoin += voiceSender.OnPlayerJoined;
        serverAPI.Event.PlayerLeave += voiceSender.OnPlayerLeave;
        serverAPI.Event.RegisterGameTickListener(voiceSender.UpdatePlayerEars, 5000);
        Debug.Log("Player events instanciated");
    }
}