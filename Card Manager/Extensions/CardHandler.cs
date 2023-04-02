using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace R3DCore.CM.Extensions
{
    public static class CardHandlerExtension
    {
        public static int GetIDOfCard(this CardHandler cardHandler, CardUpgrade cardToGet)
        {
            return (int)typeof(CardHandler).InvokeMember("GetIDOfCard",
                BindingFlags.Instance | BindingFlags.InvokeMethod |
                BindingFlags.NonPublic, null, cardHandler, new object[] { cardToGet });
        }
        public static CardUpgrade GetCardWithID(this CardHandler cardHandler, int cardID)
        {
            return (CardUpgrade)typeof(CardHandler).InvokeMember("GetCardWithID",
                BindingFlags.Instance | BindingFlags.InvokeMethod |
                BindingFlags.NonPublic, null, cardHandler, new object[] { cardID });
        }
    }
}