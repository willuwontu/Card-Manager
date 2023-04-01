using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace R3DCore
{
    public static class CardManager
    {
        private static Dictionary<string, CardInfo> _hiddenCards = new Dictionary<string, CardInfo>();
        private static Dictionary<string, CardInfo> _cards = new Dictionary<string, CardInfo>();

        public static ReadOnlyDictionary<string, CardInfo> Cards => new ReadOnlyDictionary<string, CardInfo>(_cards);
        public static ReadOnlyDictionary<string, CardInfo> HiddenCards => new ReadOnlyDictionary<string, CardInfo>(_hiddenCards);

        public static void RegisterCard(string cardName, CardUpgrade card, string modName, bool hidden = false)
        {
            if (hidden)
            {
                _hiddenCards.Add((modName + "_" + cardName).Sanitize(), new CardInfo(card, cardName, modName, hidden));
            }
        }

        public class CardInfo
        {
            public CardUpgrade card;
            public string cardName;
            public string modName;
            public bool hidden;

            public CardInfo(CardUpgrade card, string cardName, string modName, bool hidden)
            {
                this.card = card;
                this.cardName = cardName;
                this.modName = modName;
                this.hidden = hidden;
            }
        }
    }
}
