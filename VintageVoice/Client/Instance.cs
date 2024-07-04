using System;
using Vintagestory.API.Client;

namespace VintageVoice.Client;
public class Instance
{
    ICoreClientAPI clientAPI;
    public IClientNetworkChannel communicationChannel;
    readonly VoiceRecorder voiceRecorder = new();

    public void Init(ICoreClientAPI api)
    {
        clientAPI = api;
        communicationChannel = api.Network.RegisterChannel("VintageVoice_VoiceBuffer").RegisterMessageType(typeof(byte[]));
        voiceRecorder.Init(communicationChannel);
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