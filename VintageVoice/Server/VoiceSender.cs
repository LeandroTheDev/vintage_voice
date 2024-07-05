using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VintageVoice.Server;
class VoiceSender
{
    IServerNetworkChannel communicationChannel;
    IServerWorldAccessor world;
    readonly Dictionary<string, Dictionary<string, double>> playerEars = [];
    readonly List<string> playerMouth = [];

    public void Init(IServerWorldAccessor _world, IServerNetworkChannel communication)
    {
        world = _world;
        communicationChannel = communication;
        Debug.Log("Voice Sender initialized");
    }

    public void OnPlayerJoined(IServerPlayer byPlayer)
    => playerEars.Add(byPlayer.PlayerUID, []);

    public void OnPlayerLeave(IServerPlayer byPlayer)
    => playerEars.Remove(byPlayer.PlayerUID);

    /// This will automatically update the ears from all players
    public void UpdatePlayerEars(float _)
    {
        // Running on secondary thread to not lag the server
        // this functions can cause much lag in very high player counts
        Task.Run(() =>
        {
            // Teory
            // We swiping all players, the player will check
            // every ears if he is in the proximity
            // if not will be removed
            // if in will be update the volume based on distance

            // Swipe all online players
            foreach (IPlayer player in world.AllOnlinePlayers)
            {
                int maxHorizontal = 50;
                int maxVertical = 20;
                // Swipe all ears
                foreach (KeyValuePair<string, Dictionary<string, double>> ear in playerEars)
                {
                    // Check if is the same player
                    if (ear.Key == player.PlayerUID) continue;

                    IPlayer otherPlayer = world.PlayerByUid(ear.Key);
                    // Check if player is online
                    if (otherPlayer == null)
                    {
                        // Remove it from ears
                        playerEars.Remove(ear.Key);
                        continue;
                    }

                    // Getting x diff
                    double xDiff;
                    if (otherPlayer.Entity.Pos.X > player.Entity.Pos.X)
                        xDiff = otherPlayer.Entity.Pos.X - player.Entity.Pos.X;
                    else
                        xDiff = player.Entity.Pos.X - otherPlayer.Entity.Pos.X;

                    // Getting y diff
                    double yDiff;
                    if (otherPlayer.Entity.Pos.Y > player.Entity.Pos.Y)
                        yDiff = otherPlayer.Entity.Pos.Y - player.Entity.Pos.Y;
                    else
                        yDiff = player.Entity.Pos.Y - otherPlayer.Entity.Pos.Y;

                    // Getting z diff
                    double zDiff;
                    if (otherPlayer.Entity.Pos.Z > player.Entity.Pos.Z)
                        zDiff = otherPlayer.Entity.Pos.Z - player.Entity.Pos.Z;
                    else
                        zDiff = player.Entity.Pos.Z - otherPlayer.Entity.Pos.Z;

                    // Player is not in the proximity
                    if (xDiff > maxHorizontal && yDiff > maxVertical && zDiff > maxHorizontal)
                    {
                        // Remove if exist
                        playerEars[ear.Key].Remove(player.PlayerUID);
                        continue;
                    }

                    // Player is on proximity lets make the calculation

                    // Getting Horizontal distance
                    double horizontalDistance;
                    if (xDiff > zDiff) horizontalDistance = xDiff;
                    else horizontalDistance = zDiff;

                    // Getting Vertical distance
                    double verticalDistance = yDiff;

                    // Getting volumes
                    double volumeHorizontal = Math.Clamp(horizontalDistance, 0, maxHorizontal) / maxHorizontal;
                    volumeHorizontal = (1.0 - volumeHorizontal) * 100;
                    double volumeVertical = Math.Clamp(verticalDistance, 0, maxVertical) / maxVertical;
                    volumeVertical = (1.0 - volumeVertical) * 100;

                    // Setting the new volume to the ear
                    playerEars[ear.Key].Remove(player.PlayerUID);
                    playerEars[ear.Key].Add(player.PlayerUID, (volumeHorizontal + volumeVertical) / 2);
                }
            }
        });
    }

    /// Handling player voice
    /// Sending data:
    /// 0 = audio bytes
    /// 1 = fromPlayer UID
    /// 2 = volume for the listener
    ///
    /// This cannot be separeted thread, because we dont want have the voice delayed
    /// or laggy
    public void OnCommunicationReceived(IServerPlayer fromPlayer, byte[] packet)
    {
        // Check if player is talking
        if (!playerMouth.Contains(fromPlayer.PlayerUID))
        {
            Debug.Log($"WARNING: {fromPlayer.PlayerName} is sending a packet voice but hes not talking");
            return;
        }

        List<object> data = [];
        data[0] = packet;
        data[1] = fromPlayer.PlayerUID;

        // Swipe all ears from the player
        foreach (KeyValuePair<string, double> ear in playerEars[fromPlayer.PlayerUID])
        {
            // Get listener
            IPlayer playerListening = world.PlayerByUid(ear.Key);
            if (playerListening is IServerPlayer)
            {
                // Update the volume value
                data[2] = ear.Value;
                // Send the data to the client
                communicationChannel.SendPacket(data, playerListening as IServerPlayer);
            }
        }
    }

    public void OnInfoReceived(IServerPlayer fromPlayer, string packet)
    {
        // Start talking
        if (packet == "start_talking")
        {
            // Check if is already talking
            if (playerMouth.Contains(fromPlayer.PlayerUID))
            {
                Debug.Log($"ERROR: {fromPlayer.PlayerName} is trying to start a talk, but hes already talking");
                return;
            }
            // Otherwises add it to the mouth
            playerMouth.Add(fromPlayer.PlayerUID);
        }
        // Start talking
        else if (packet == "stop_talking")
        {
            // Check if not talking
            if (!playerMouth.Contains(fromPlayer.PlayerUID))
            {
                Debug.Log($"ERROR: {fromPlayer.PlayerName} is trying to stop talking, but hes not talking");
                return;
            }
            // Otherwises remove it from mouth
            playerMouth.Remove(fromPlayer.PlayerUID);
        }
    }
}