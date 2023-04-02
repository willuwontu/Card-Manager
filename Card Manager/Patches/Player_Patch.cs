using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace R3DCore.CM.Patches
{
    [HarmonyPatch(typeof(Player))]
    class Player_Patch
    {
        // Need to switch this to a transpiler later on.
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        [HarmonyPatch("AddCard")]
        static bool AddCard(CardUpgrade card)
        {
            if (card == null)
            {
                return false;
            }

            return true;
        }

        [HarmonyPostfix()]
        [HarmonyPatch("AddCard")]
        static void ReAddCard()
        {

        }
    }
}