using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedMedical.Commands
{
    public class CommandDrag : Command
    {
        protected override void execute(CSteamID executorID, string parameter)
        {
            if (MedicalManager.DraggedPlayers.ContainsKey(executorID))
            {
                MedicalManager.DraggedPlayers.Remove(executorID);
                return;
            }

            Player ply = PlayerTool.getPlayer(executorID);

            List<Player> nearbyPlayers = new List<Player>();

            PlayerTool.getPlayersInRadius(ply.movement.transform.position, 10f, nearbyPlayers);

            foreach (Player p in nearbyPlayers)
            {
                if (MedicalManager.DownedPlayers.Keys.Contains(p.channel.owner.playerID.steamID))
                {
                    MedicalManager.DraggedPlayers.Add(ply.channel.owner.playerID.steamID, p.channel.owner.playerID.steamID);
                }
            }
        }

        public CommandDrag()
        {
            this.localization = new Local();
            this._command = "drag";
            this._info = "drag";
            this._help = "Drag a downed player";
        }
    }
}
