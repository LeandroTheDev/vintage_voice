using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VintageVoice.Server;
class VoiceSender
{
    IServerWorldAccessor world;
    Dictionary<string, Dictionary<string, double>> playerEars = [];

    public void Init(IServerWorldAccessor _world)
    {
        world = _world;
    }

    public void OnPlayerJoined(IServerPlayer byPlayer)
    => playerEars.Add(byPlayer.PlayerUID, []);

    public void OnPlayerLeave(IServerPlayer byPlayer)
    => playerEars.Remove(byPlayer.PlayerUID);

    public void UpdatePlayerEars(float _)
    {
        // Teory
        // We sipeing all the player, the player will check
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
    }

    public void OnCommunicationReceived(IServerPlayer fromPlayer, byte[] packet)
    {

    }
}