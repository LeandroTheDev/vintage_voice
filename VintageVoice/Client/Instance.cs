using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace VintageVoice.Client;
public class Instance
{
    ICoreClientAPI clientAPI;
    public IClientNetworkChannel communicationChannel;
    public IClientNetworkChannel infoChannel;
    readonly VoiceRecorder voiceRecorder = new();
    readonly VoiceSound voiceSound = new();

    public void Init(ICoreClientAPI api)
    {
        clientAPI = api;
        voiceSound.Init(clientAPI.World);
        
        communicationChannel = api.Network.RegisterChannel("VintageVoice_VoiceBuffer").RegisterMessageType(typeof(byte[]));
        communicationChannel.SetMessageHandler<List<object>>(voiceSound.OnVoiceReceived);
        infoChannel = api.Network.RegisterChannel("VintageVoice_VoiceInfo").RegisterMessageType(typeof(string));
        Debug.Log("Channels initialized");

        voiceRecorder.Init(communicationChannel, infoChannel);

        clientAPI.Input.RegisterHotKey("pushtotalk", "Push to talk", GlKeys.C, HotkeyType.GUIOrOtherControls);
        clientAPI.Input.SetHotKeyHandler("pushtotalk", HotKeyListener);

        Debug.Log("Hotkeys registered");
    }

    private bool HotKeyListener(KeyCombination key)
    {
        voiceRecorder.StartRecording();
        return true;
    }
}