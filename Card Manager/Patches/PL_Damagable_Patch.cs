using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace R3DCore.CM.Patches
{
    [HarmonyPatch(typeof(PL_Damagable))]
    class PL_Damagable_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Revive")]
        static void Revive(Player ___player)
        {
            NotificationHandler.instance.PlayNotification($"{___player.refs.view.Owner.NickName} died.", 5f);
        }
    }
}