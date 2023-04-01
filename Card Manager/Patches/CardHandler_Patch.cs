using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace R3DCore.CM.Patches
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
            __result = CardManager.GetRandomCard();

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void Awake(CardHandler __instance)
        {
            foreach (var card in __instance.cards)
            {
                if (!CardManager.Cards.Select(ci => ci.card).Contains(card))
                {
                    CardManager.RegisterCard(GetVanillaCardName(card), card, "Vanilla", CardHandler.instance.cards.Where(c => c == card).Count(), false);
                }
            }

            CardHandler.instance.cards = CardManager.Cards.OrderBy(ci => ci.modName).ThenBy(ci => ci.cardName).Select(ci => ci.card).ToList();
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

        private static string GetVanillaCardName(CardUpgrade card)
        {
            string output = card.gameObject.name;

            if (vanillaNameMap.ContainsKey(output))
            {
                output = vanillaNameMap[output];
            }
            else
            {
                output = output.Replace("C_", string.Empty);
            }

            return output;
        }

        private static Dictionary<string, string> vanillaNameMap = new Dictionary<string, string>() { { "C_ExplosiveP", "Explosive Bullets" }, { "C_ToxicCloud", "Toxic Cloud" }, { "C_ColdBullets", "Cold Bullets" }, { "C_Wind up", "Wind Up" } };
    }
}