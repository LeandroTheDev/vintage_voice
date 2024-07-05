using System.Collections.Generic;
using Vintagestory.API.Client;

namespace VintageVoice.Client;
public class VoiceSound
{
    IClientWorldAccessor world;
    /// 0 = Voice Bytes
    /// 1 = Voice Volume
    /// 2 = Timeout
    Dictionary<string, List<object>> playerVoices = [];

    public void Init(IClientWorldAccessor _world) {
        world = _world;
        Debug.Log("Voice Sound initialized");
    }

    /// 0 = Voice Bytes
    /// 1 = From Player UID
    /// 2 = Voice Volume
    public void OnVoiceReceived(List<object> packet)
    {
        
    }
}