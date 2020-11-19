using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedMedical.Patches
{
    [HarmonyPatch(typeof(PlayerEquipment))]
    [HarmonyPatch("tellEquip")]
    class TellEquipPatch
    {
        public static void Postfix(CSteamID steamID, byte page, byte x, byte y, ushort id, byte newQuality, byte[] newState, PlayerEquipment __instance)
        {
            CommandWindow.Log("TellEquipPatch Fired");
            ItemAsset item = (ItemAsset)Assets.find(EAssetType.ITEM, id);
            if (item is null) return;

            FieldInfo field = item.GetType().GetField("_animations", BindingFlags.Instance | BindingFlags.NonPublic);
            AnimationClip[] animations = (AnimationClip[])field.GetValue(item);
            CommandWindow.Log($"animations: {animations} : {animations.Length}");

            for (int i = 0; i < animations.Length; i++)
            {
                __instance.channel.owner.player.animator.removeAnimation(animations[i]);   
            }
        }
    }
}
