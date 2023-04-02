using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace R3DCore.CM.Patches
{
    [HarmonyPatch(typeof(NotificationHandler))]
    class NotificationHandler_Patch
    {

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void Awake(NotificationHandler __instance)
        {
            __instance.gameObject.AddComponent<NotificationHandlerMessageAdjustment>();
        }
    }

    class NotificationHandlerMessageAdjustment : MonoBehaviour
    {
        Dictionary<string, string> CardObjectNameToCardNameMap => CardManager.Cards.ToDictionary(ci => ci.card.gameObject.name, ci => ci.cardName);

        private void OnTransformChildrenChanged()
        {
            this.StartCoroutine(ReplaceText());
        }

        private IEnumerator ReplaceText()
        {
            yield return null;

            foreach (var tmp in this.gameObject.GetComponentsInChildren<TextMeshProUGUI>(false))
            {
                foreach (var kvp in CardObjectNameToCardNameMap)
                {
                    if (tmp.text.Contains(kvp.Key))
                    {
                        tmp.text = tmp.text.Replace(kvp.Key, kvp.Value);
                    }
                }
            }

            yield break;
        }
    }
}