using HarmonyLib;
using UnityEngine;

namespace R3DCore.Patches
{
    [HarmonyPatch(typeof(CardHandler))]
    class CardHandler_Patch
    {
        // This should be the only patch to this method.
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        [HarmonyPatch("GetRandomCard")]
        static bool GetRandomCard(ref CardUpgrade __result)
        {
            __result = CardHandler.instance.cards[UnityEngine.Random.Range(0, CardHandler.instance.cards.Count)];

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void Awake()
        {

        }

        // Need to switch this to a transpiler later on.
        [HarmonyPrefix]
        [HarmonyPatch("GetCardWithID")]
        static bool GetCardWithID(int cardID, ref CardUpgrade __result)
        {
            if (cardID == -1)
            {
                __result = null;
                return false;
            }

            return true;
        }
    }
}