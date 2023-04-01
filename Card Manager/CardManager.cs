using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using R3DCore.CM.Extensions;

namespace R3DCore
{
    public static class CardManager
    {
        public static readonly ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "CardManager.cfg"), true);
        private static List<CardInfo> _cards = new List<CardInfo>();
        public static ReadOnlyCollection<CardInfo> Cards => new ReadOnlyCollection<CardInfo>(_cards);

        public static void RegisterCard(string cardName, CardUpgrade card, string modName, int rarity, bool hidden = false, bool allowMultiple = true)
        {
            if (Cards.Select(ci => ci.card).Contains(card))
            {
                throw new ArgumentException("Cards may only be registered once.");
            }

            _cards.Add(new CardInfo(card, cardName, modName, rarity, hidden, allowMultiple));
        }

        public static CardUpgrade GetRandomCard()
        {
            CardUpgrade card = null;

            CardInfo[] availableCards = Cards.Where(c => c.enabled && !c.hidden).ToArray();

            if (availableCards.Length > 0)
            {
                int sum = availableCards.Sum(c => c.rarity);

                var rand = UnityEngine.Random.Range(0, sum);

                for (int i = 0; i < availableCards.Length; i++)
                {
                    rand -= availableCards[i].rarity;
                    if (rand <= 0)
                    {
                        card = availableCards[i].card;
                        break;
                    }
                }
            }

            return card;
        }

        public static CardUpgrade GetRandomCard(Func<CardUpgrade, bool> condition)
        {
            CardUpgrade card = null;

            if (condition == null)
            {
                return GetRandomCard();
            }

            CardInfo[] availableCards = Cards.Where(c => c.enabled && !c.hidden && condition(c.card)).ToArray();

            if (availableCards.Length > 0)
            {
                int sum = availableCards.Sum(c => c.rarity);

                var rand = UnityEngine.Random.Range(0, sum);

                for (int i = 0; i < availableCards.Length; i++)
                {
                    rand -= availableCards[i].rarity;
                    if (rand <= 0)
                    {
                        card = availableCards[i].card;
                        break;
                    }
                }
            }

            return card;
        }

        public static void AddCardToPlayer(Player player, CardUpgrade card)
        {
            player.refs.view.RPC("RPCA_ADDCARDWITHID", RpcTarget.All, new object[]
            {
                CardHandler.instance.GetIDOfCard(card)
            });
        }

        public class CardInfo
        {
            public CardUpgrade card;
            public string cardName;
            public string modName;
            public bool allowMultiple = true;
            public bool hidden;
            public bool enabled = true;
            public int rarity;

            public CardInfo(CardUpgrade card, string cardName, string modName, int rarity, bool hidden, bool allowMultiple)
            {
                this.card = card;
                this.cardName = cardName;
                this.modName = modName;
                this.rarity = rarity;
                this.hidden = hidden;
                this.allowMultiple = allowMultiple;
            }
        }
    }
}
