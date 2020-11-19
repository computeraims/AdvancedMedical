using AdvancedMedical.Commands;
using AdvancedMedical.Utils;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedMedical
{
    class MedicalManager : MonoBehaviour
    {
        public static Dictionary<CSteamID, long> DownedPlayers;
        public static Dictionary<CSteamID, CSteamID> DraggedPlayers;
        public static Dictionary<CSteamID, long> RecentlyDownedPlayers;

        public void Awake()
        {
            Commander.register(new CommandDrag());

            DownedPlayers = new Dictionary<CSteamID, long>();
            DraggedPlayers = new Dictionary<CSteamID, CSteamID>();
            RecentlyDownedPlayers = new Dictionary<CSteamID, long>();

            ChatManager.onCheckPermissions += OnCheckedPermissions;

            PlayerLife.onPlayerDied += OnPlayerDied;
            DamageTool.damagePlayerRequested += OnDamagePlayerRequested;
            UseableConsumeable.onPerformedAid += OnPerformedAid;

            AsyncHelper.Schedule("CheckDrag", CheckDrag, 100);

            if (Main.Config.bleedOutTime != 0)
            {
                AsyncHelper.Schedule("CheckBleedout", CheckBleedout, 1000);
            }

            if (Main.Config.recentlyDownedTime != 0)
            {
                AsyncHelper.Schedule("CheckRecentlyDowned", CheckRecentlyDowned, 1000);
            }

            Console.WriteLine("MedicalManager loaded");


        }

        #region Events
        private void OnCheckedPermissions(SteamPlayer player, string text, ref bool shouldExecuteCommand, ref bool shouldList)
        {
            if (text.StartsWith("/drag"))
            {
                shouldExecuteCommand = true;
            }
        }

        private void OnPlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
        {
            
            if (IsPlayerDown(sender.channel.owner.playerID.steamID))
            {
                DownedPlayers.Remove(sender.channel.owner.playerID.steamID);
                if (IsPlayerBeingDragged(sender.channel.owner.playerID.steamID))
                {
                    DraggedPlayers.Remove(sender.channel.owner.playerID.steamID);
                }

                sender.channel.owner.player.movement.sendPluginSpeedMultiplier(1);
                sender.channel.owner.player.movement.sendPluginJumpMultiplier(1);
            }
        }

        private void OnDamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (parameters.damage >= parameters.player.life.health && !IsPlayerDown(parameters.player.channel.owner.playerID.steamID))
            {
                Player ply = parameters.player;
                if (ply is null) return;

                if (Main.Config.explosionInstaKill && parameters.cause == EDeathCause.CHARGE ||
                    Main.Config.explosionInstaKill && parameters.cause == EDeathCause.GRENADE ||
                    Main.Config.explosionInstaKill && parameters.cause == EDeathCause.LANDMINE ||
                    Main.Config.explosionInstaKill && parameters.cause == EDeathCause.MISSILE)
                {
                    shouldAllow = true;
                    return;
                }

                if (IsRecentlyDowned(parameters.player.channel.owner.playerID.steamID))
                {
                    shouldAllow = true;
                    return;
                }

                DownPlayer(ply.channel.owner.playerID.steamID);
                shouldAllow = false;
            }

            if (IsPlayerDown(parameters.player.channel.owner.playerID.steamID) && (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - DownedPlayers[parameters.player.channel.owner.playerID.steamID] <= Main.Config.downedProtectionTime)
            {
                shouldAllow = false;
            }
        }

        private void OnPerformedAid(Player instigator, Player target)
        {
            if (instigator is null) return;
            if (target is null) return;

            if (IsPlayerDown(target.channel.owner.playerID.steamID))
            {
                RevivePlayer(target.channel.owner.playerID.steamID);
            }
        }
        #endregion

        #region Thread Checks
        private async Task CheckDrag()
        {
            foreach (KeyValuePair<CSteamID, CSteamID> pair in DraggedPlayers.Keys.ToDictionary(_ => _, _ => DraggedPlayers[_]))
            {
                Player drager = PlayerTool.getPlayer(pair.Key);
                if (drager is null)
                {
                    return;
                }

                Player dragee = PlayerTool.getPlayer(pair.Value);
                if (dragee is null)
                {
                    return;
                }

                dragee.movement.transform.position = drager.movement.transform.position;
            }
        }

        private async Task CheckBleedout()
        {
            long now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            foreach (CSteamID key in DownedPlayers.Keys.ToList())
            {
                if (now - DownedPlayers[key] >= Main.Config.bleedOutTime)
                {
                    Player ply = PlayerTool.getPlayer(key);
                    if (ply is null) return;

                    if (UnityEngine.Random.value > Main.Config.survivalChance)
                    {
                        UnityThread.executeInUpdate(() =>
                        {
                            RevivePlayer(key);
                        });
                        return;
                    }

                    EPlayerKill kill;
                    UnityThread.executeInUpdate(() =>
                    {
                        ply.life.askDamage((byte)255, new Vector3(0, 0, 0), EDeathCause.BLEEDING, ELimb.SKULL, CSteamID.Nil, out kill);
                    });
                }
            }
        }

        private async Task CheckRecentlyDowned()
        {
            long now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            foreach (CSteamID key in RecentlyDownedPlayers.Keys.ToList())
            {
                if (now - RecentlyDownedPlayers[key] >= Main.Config.recentlyDownedTime)
                {
                    Player ply = PlayerTool.getPlayer(key);
                    if (ply is null) return;

                    RecentlyDownedPlayers.Remove(key);
                }
            }
        }
        #endregion

        #region API
        public static bool IsPlayerDown(CSteamID steamID)
        {
            if (DownedPlayers.ContainsKey(steamID))
            {
                return true;
            }
            return false;
        }

        private bool IsPlayerBeingDragged(CSteamID steamID)
        {
            if (DraggedPlayers.ContainsKey(steamID))
            {
                return true;
            }
            return false;
        }

        public static bool IsRecentlyDowned(CSteamID steamID)
        {
            if (RecentlyDownedPlayers.ContainsKey(steamID))
            {
                return true;
            }
            return false;
        }

        private void DownPlayer(CSteamID steamID)
        {
            Player ply = PlayerTool.getPlayer(steamID);
            if (ply is null) return;

            ply.stance.stance = EPlayerStance.PRONE;
            ply.stance.checkStance(EPlayerStance.PRONE);

            ply.life.serverSetBleeding(false);
            ply.life.tellHealth(ply.channel.owner.playerID.steamID, (byte)Main.Config.downedHealth);
            ply.movement.sendPluginSpeedMultiplier(Main.Config.downedMovementMultiplier);
            ply.movement.sendPluginJumpMultiplier(0);
            ply.equipment.dequip();

            DownedPlayers.Add(steamID, (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

            if (Main.Config.recentlyDownedTime != 0)
            {
                RecentlyDownedPlayers.Add(steamID, (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + Main.Config.recentlyDownedTime);
            }
        }

        private void RevivePlayer(CSteamID steamID)
        {
            Player ply = PlayerTool.getPlayer(steamID);
            if (ply is null) return;

            DownedPlayers.Remove(steamID);
            if (IsPlayerBeingDragged(steamID))
            {
                DraggedPlayers.Remove(steamID);
            }

            ply.stance.stance = EPlayerStance.STAND;
            ply.stance.checkStance(EPlayerStance.STAND);

            ply.movement.sendPluginSpeedMultiplier(1);
            ply.movement.sendPluginJumpMultiplier(1);
            ply.life.tellHealth(steamID, (byte)Main.Config.revivedHealth);
        }
        #endregion
    }
}
