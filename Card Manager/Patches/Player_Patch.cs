using HarmonyLib;
using System;

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

        [HarmonyPrefix]
        [HarmonyPatch("RemoveCards")]
        static void RemoveCards(Player __instance)
        {
            foreach (CardUpgrade card in __instance.cards)
            {
                try
                {
                    CardManager.OnCardRemovedFromPlayer(__instance, card);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void Awake(Player __instance)
        {
            var cardHandler = __instance.gameObject.GetOrAddComponent<PL_CardHandler>();
        }
    }
}