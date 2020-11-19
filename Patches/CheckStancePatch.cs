using AdvancedMedical.Utils;
using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedMedical.Patches
{
    [HarmonyPatch(typeof(PlayerStance), "checkStance", new Type[] { typeof(EPlayerStance), typeof(bool) })]
    class CheckStancePatch
    {
        public static void Postfix(EPlayerStance newStance, bool all, PlayerStance __instance)
        {
            if (MedicalManager.IsPlayerDown(__instance.channel.owner.playerID.steamID) && __instance.player.stance.stance != EPlayerStance.PRONE)
            {
                UnityThread.executeInUpdate(() =>
                {
                    __instance.player.stance.stance = EPlayerStance.PRONE;
                    __instance.player.stance.checkStance(EPlayerStance.PRONE);
                });

                return;
            }
        }
    }
}
