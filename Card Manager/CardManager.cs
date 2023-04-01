using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace R3DCore
{
    public static class CardManager
    {
        //private static List<CardInfo> _hiddenCards = new List<CardInfo>();
        private static List<CardInfo> _cards = new List<CardInfo>();

        //public static ReadOnlyCollection<CardInfo> AllCards => new ReadOnlyCollection<CardInfo>(_cards.Concat(_hiddenCards).ToList());
        public static ReadOnlyCollection<CardInfo> Cards => new ReadOnlyCollection<CardInfo>(_cards);
        //public static ReadOnlyCollection<CardInfo> HiddenCards => new ReadOnlyCollection<CardInfo>(_hiddenCards);

        public static void RegisterCard(string cardName, CardUpgrade card, string modName, bool hidden = false)
        {
            if (Cards.Select(ci => ci.card).Contains(card))
            {
                throw new ArgumentException("Cards may only be registered once.");
            }

            _cards.Add(new CardInfo(card, cardName, modName, hidden));
        }

        public class CardInfo
        {
            public CardUpgrade card;
            public string cardName;
            public string modName;
            public bool hidden;
            public bool enabled = true;

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
