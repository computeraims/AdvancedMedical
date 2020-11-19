using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedMedical.Patches
{
    [HarmonyPatch(typeof(PlayerEquipment))]
    [HarmonyPatch("tellEquip")]
    class AskEquipPatch
    {
        public static bool Prefix(CSteamID steamID, byte page, byte x, byte y, ushort id, byte newQuality, byte[] newState)
        {
            return true;
        }


    }
}
