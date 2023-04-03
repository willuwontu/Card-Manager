using BepInEx;
using BepInEx.Configuration;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using R3DCore.CM.Extensions;
using UnityEngine;
using Photon.Realtime;

namespace R3DCore
{
    public static class CardManager
    {
        public static readonly ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "CardManager.cfg"), true);
        private static List<CardInfo> _cards = new List<CardInfo>();
        public static ReadOnlyCollection<CardInfo> Cards => new ReadOnlyCollection<CardInfo>(_cards);
        public static ReadOnlyCollection<CardUpgrade> CardUpgrades => new ReadOnlyCollection<CardUpgrade>(Cards.Select(c => c.card).ToList());

        public static CardInfo RegisterCard(string cardName, CardUpgrade card, string modName, int weight, bool canBeReassigned = true, bool hidden = false, bool allowMultiple = true)
        {
            if (Cards.Select(ci => ci.card).Contains(card))
            {
                throw new ArgumentException("Cards may only be registered once.");
            }

            CardInfo cardInfo = new CardInfo(card, cardName, modName, weight, canBeReassigned, hidden, allowMultiple);

            _cards.Add(cardInfo);
            _cards = _cards.OrderBy(ci => ci.modName).ThenBy(ci => ci.cardName).ThenBy(ci => ci.weight).ToList();
            CardHandler.instance.cards = CardManager.Cards.OrderBy(ci => ci.modName).ThenBy(ci => ci.cardName).ThenBy(ci => ci.weight).Select(ci => ci.card).ToList();

            return cardInfo;
        }

        public static CardInfo GetCardInfoFromCard(CardUpgrade card)
        {
            if (card == null)
            {
                return null;
            }

            CardInfo output = null;

            if (Cards.Select(c => c.card).Contains(card))
            {
                output = Cards.Where(c => c.card == card).FirstOrDefault();
            }

            return output;
        }

        #region GetRandomCard

        public static CardUpgrade GetRandomCard(CardUpgrade[] cards)
        {
            CardUpgrade card = null;

            if (cards.Length > 0)
            {
                CardInfo[] cardInfos = cards.Select(c => { var ci = GetCardInfoFromCard(c); UnityEngine.Debug.Log(ci.cardName + ": " + ci.weight) ; return ci; }).ToArray();

                int sum = cardInfos.Sum(c => c.weight);

                var rand = UnityEngine.Random.Range(0, sum+1);

                for (int i = 0; i < cardInfos.Length; i++)
                {
                    rand -= cardInfos[i].weight;
                    if (rand <= 0)
                    {
                        card = cardInfos[i].card;
                        break;
                    }
                }
            }

            return card;
        }

        public static CardUpgrade GetRandomCard()
        {
            CardUpgrade[] availableCards = Cards.Where(c => c.enabled && !c.hidden).Select(c => c.card).ToArray();

            return GetRandomCard(availableCards);
        }

        public static CardUpgrade GetRandomCard(Player player)
        {
            if (player == null)
            {
                return GetRandomCard();
            }

            CardUpgrade[] availableCards = Cards.Where(c => c.enabled && !c.hidden && PlayerIsAllowedCard(player, c.card)).Select(c => c.card).ToArray();

            return GetRandomCard(availableCards);
        }

        public static CardUpgrade GetRandomCard(Func<CardUpgrade, bool> condition)
        {
            if (condition == null)
            {
                return GetRandomCard();
            }

            CardUpgrade[] availableCards = Cards.Where(c => c.enabled && !c.hidden && condition(c.card)).Select(c => c.card).ToArray();

            return GetRandomCard(availableCards);
        }

        public static CardUpgrade GetRandomCard(Player player, Func<CardUpgrade, bool> condition)
        {
            if (condition == null)
            {
                return GetRandomCard(player);
            }

            if (player == null)
            {
                return GetRandomCard(condition);
            }

            CardUpgrade[] availableCards = Cards.Where(c => c.enabled && !c.hidden && PlayerIsAllowedCard(player, c.card) && condition(c.card)).Select(c => c.card).ToArray();

            return GetRandomCard(availableCards);
        }

        public static CardUpgrade GetRandomCard(Player player, Func<Player, CardUpgrade, bool> condition)
        {
            if (condition == null)
            {
                return GetRandomCard(player);
            }

            CardUpgrade[] availableCards = Cards.Where(c => c.enabled && !c.hidden && PlayerIsAllowedCard(player, c.card) && condition(player, c.card)).Select(c => c.card).ToArray();

            return GetRandomCard(availableCards);
        }

        #endregion GetRandomCard

        #region CardValidation

        public static bool PlayerIsAllowedCard(Player player, CardUpgrade card)
        {
            bool output = true;

            if (player == null)
            {
                return true;
            }

            if (card == null)
            {
                return false;
            }

            if (!GetCardInfoFromCard(card).allowMultiple && player.cards.Contains(card))
            {
                return false;
            }

            bool cardsConflict = false;

            for (int i = 0; i < player.cards.Count; i++)
            {
                cardsConflict = CardsConflict(player.cards[i], card);
                if (cardsConflict)
                {
                    break;
                }
            }

            output = output && !cardsConflict;

            bool flag = true;

            for (int i = cardValidationFunctions.Keys.Count() - 1; i >= 0; i--)
            {
                var key = cardValidationFunctions.Keys.ToArray()[i];
                for (int j = cardValidationFunctions[key].Count() - 1; j >= 0; j--)
                {
                    try
                    {
                        flag = cardValidationFunctions[key][j](player,card);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"[Card Manager] Validation funciton from Mod '{key}' threw an exception when ran, see error below:");
                        UnityEngine.Debug.LogException(e);
                    }

                    if (!flag)
                    {
                        break;
                    }
                }

                if (!flag)
                {
                    break;
                }
            }

            output = output && flag;

            return output;
        }

        public static bool CardsConflict(CardUpgrade card, CardUpgrade otherCard)
        {
            CardInfo ci1 = CardManager.GetCardInfoFromCard(card);

            CardInfo ci2 = CardManager.GetCardInfoFromCard(otherCard);

            if (ci1.blacklistedCategories.Intersect(ci2.categories).Count() > 0 || ci2.blacklistedCategories.Intersect(ci1.categories).Count() > 0)
            {
                return true;
            }

            return false;
        }

        private static Dictionary<string, List<Func<Player, CardUpgrade, bool>>> cardValidationFunctions = new Dictionary<string, List<Func<Player, CardUpgrade, bool>>>();

        public static void AddCardValidationFunctions(string modname, Func<Player, CardUpgrade, bool> validationFunction)
        {
            if (!cardValidationFunctions.ContainsKey(modname))
            {
                cardValidationFunctions.Add(modname, new List<Func<Player, CardUpgrade, bool>>());
            }
            cardValidationFunctions[modname].Add(validationFunction);
        }

        #endregion CardValidation

        #region CardManipulation

        public static void AddCardToPlayer(Player player, CardUpgrade card)
        {
            player.refs.view.RPC("RPCA_ADDCARDWITHID", RpcTarget.All, new object[]
            {
                CardHandler.instance.GetIDOfCard(card)
            });
        }

        public static void AddCardsToPlayer(Player player, CardUpgrade[] cards)
        {
            foreach (var card in cards)
            {
                AddCardToPlayer(player, card);
            }
        }

        public static void SetPlayerCards(Player player, CardUpgrade[] cards)
        {
            player.refs.view.RPC(nameof(PL_CardHandler.RPCA_SetCards), RpcTarget.All, new object[] { cards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray() });
        }

        public static void RemoveAllCardsFromPlayer(Player player)
        {
            player.refs.view.RPC(nameof(PL_CardHandler.RPCA_RemoveCards), RpcTarget.All, new object[] { });
        }

        #region RemoveByIndex
        public static void RemoveCardFromPlayer(Player player, int index)
        {
            if (index >= player.cards.Count() || index < 0)
            {
                return;
            }

            List<CardUpgrade> oldCards = player.cards.ToList();
            oldCards.RemoveAt(index);

            player.refs.view.RPC(nameof(PL_CardHandler.RPCA_UpdateCards), RpcTarget.All, new object[] { oldCards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray() });
        }

        public static void RemoveCardsFromPlayer(Player player, int[] indices)
        {
            if (indices == null)
            {
                throw new NullReferenceException("Parameter int[] indices cannot be null.");
            }

            if (indices.Length == 0)
            {
                return;
            }

            CardUpgrade[] oldCards = player.cards.ToArray();
            List<CardUpgrade> newCards = new List<CardUpgrade>();

            for (int i = 0; i < oldCards.Length; i++)
            {
                if (!indices.Contains(i))
                {
                    newCards.Add(oldCards[i]);
                }
            }

            player.refs.view.RPC(nameof(PL_CardHandler.RPCA_UpdateCards), RpcTarget.All, new object[] { newCards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray() });
        }
        #endregion RemoveByIndex

        #region RemoveByCard
        public static void RemoveCardFromPlayer(Player player, CardUpgrade card, SelectionType selectionType = SelectionType.Newest)
        {
            if (card == null || !player.cards.Contains(card))
            {
                return;
            }

            CardUpgrade[] oldCards = player.cards.ToArray();
            List<CardUpgrade> newCards = new List<CardUpgrade>();
            List<int> removedIndeces = new List<int>();

            int[] indeces = Enumerable.Range(0, oldCards.Length).Where(idx => oldCards[idx] == card).ToArray();

            if (indeces.Length > 0)
            {
                switch (selectionType) 
                { 
                    case SelectionType.Newest:
                        removedIndeces.Add(indeces[indeces.Length - 1]); 
                        break;
                    case SelectionType.All:
                        removedIndeces.AddRange(indeces); 
                        break;
                    case SelectionType.Oldest:
                        removedIndeces.Add(indeces[0]);
                        break;
                    case SelectionType.Random:
                        removedIndeces.Add(indeces[UnityEngine.Random.Range(0, indeces.Length)]);
                        break;
                }
            }

            for (int i = 0; i < oldCards.Length; i++)
            {
                if (!removedIndeces.Contains(i))
                {
                    newCards.Add(oldCards[i]);
                }
            }

            player.refs.view.RPC(nameof(PL_CardHandler.RPCA_UpdateCards), RpcTarget.All, new object[] { newCards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray() });
        }

        public static void RemoveCardsFromPlayer(Player player, CardUpgrade[] cards, SelectionType selectionType = SelectionType.Newest)
        {
            if (cards == null)
            {
                throw new NullReferenceException("Parameter CardUpgrade[] cards cannot be null.");
            }

            if (cards.Length < 1 || player.cards.Intersect(cards).Count() < 1)
            {
                return;
            }

            CardUpgrade[] oldCards = player.cards.ToArray();
            List<CardUpgrade> newCards = new List<CardUpgrade>();
            List<int> removedIndeces = new List<int>();

            for (int i = 0; i < cards.Length; i++)
            {
                CardUpgrade card = cards[i];

                int[] indeces = Enumerable.Range(0, oldCards.Length).Where(idx => oldCards[idx] == card).ToArray();

                if (indeces.Length > 0)
                {
                    switch (selectionType)
                    {
                        case SelectionType.Newest:
                            removedIndeces.Add(indeces[indeces.Length - 1]);
                            break;
                        case SelectionType.All:
                            removedIndeces.AddRange(indeces);
                            break;
                        case SelectionType.Oldest:
                            removedIndeces.Add(indeces[0]);
                            break;
                        case SelectionType.Random:
                            removedIndeces.Add(indeces[UnityEngine.Random.Range(0, indeces.Length)]);
                            break;
                    }
                }
            }

            for (int i = 0; i < oldCards.Length; i++)
            {
                if (!removedIndeces.Contains(i))
                {
                    newCards.Add(oldCards[i]);
                }
            }

            player.refs.view.RPC(nameof(PL_CardHandler.RPCA_UpdateCards), RpcTarget.All, new object[] { newCards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray() });
        }

        public static void RemoveCardsFromPlayer(Player player, CardUpgrade[] cards, SelectionType[] selectionType)
        {
            if (selectionType == null)
            {
                throw new NullReferenceException("Parameter SelectionType[] selectionType cannot be null.");
            }

            if (cards == null)
            {
                throw new NullReferenceException("Parameter CardUpgrade[] cards cannot be null.");
            }

            if (selectionType.Length != cards.Length)
            {
                throw new ArgumentException("Parameters CardUpgrade[] cards and SelectionType[] selectionType must be arrays of the same size.");
            }

            if (cards.Length < 1 || player.cards.Intersect(cards).Count() < 1)
            {
                return;
            }

            CardUpgrade[] oldCards = player.cards.ToArray();
            List<CardUpgrade> newCards = new List<CardUpgrade>();
            List<int> removedIndeces = new List<int>();

            for (int i = 0; i < cards.Length; i++)
            {
                CardUpgrade card = cards[i];

                int[] indeces = Enumerable.Range(0, oldCards.Length).Where(idx => oldCards[idx] == card).ToArray();

                if (indeces.Length > 0)
                {
                    switch (selectionType[i])
                    {
                        case SelectionType.Newest:
                            removedIndeces.Add(indeces[indeces.Length - 1]);
                            break;
                        case SelectionType.All:
                            removedIndeces.AddRange(indeces);
                            break;
                        case SelectionType.Oldest:
                            removedIndeces.Add(indeces[0]);
                            break;
                        case SelectionType.Random:
                            removedIndeces.Add(indeces[UnityEngine.Random.Range(0, indeces.Length)]);
                            break;
                    }
                }
            }

            for (int i = 0; i < oldCards.Length; i++)
            {
                if (!removedIndeces.Contains(i))
                {
                    newCards.Add(oldCards[i]);
                }
            }

            player.refs.view.RPC(nameof(PL_CardHandler.RPCA_UpdateCards), RpcTarget.All, new object[] { newCards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray() });
        }

        #endregion RemoveByCard

        #endregion CardManipulation

        public static void OnCardRemovedFromPlayer(Player player, CardUpgrade card)
        {
            IOnRemoveFromPlayer[] components = card.GetComponents<IOnRemoveFromPlayer>();
            for (int i = 0; i < components.Length; i++)
            {
                components[i].OnRemoveFromPlayer(player);
            }
        }

        public static void OnReapplyCardToPlayer(Player player, CardUpgrade card)
        {
            IOnReapplyToPlayer[] components = card.GetComponents<IOnReapplyToPlayer>();
            for (int i = 0; i < components.Length;i++)
            {
                components[i].OnReapplyToPlayer(player);
            }
        }

        public enum SelectionType
        {
            All,
            Oldest,
            Newest,
            Random
        }

        public class CardInfo
        {
            public readonly CardUpgrade card;
            public readonly string cardName;
            public readonly string modName;
            public readonly bool allowMultiple = true;
            public readonly bool hidden;
            public bool enabled = true;
            public int weight;
            public bool canBeReassigned;
            public CardCategory[] categories = new CardCategory[0];
            public CardCategory[] blacklistedCategories = new CardCategory[0];

            public CardInfo(CardUpgrade card, string cardName, string modName, int weight, bool canBeReassigned, bool hidden, bool allowMultiple)
            {
                this.card = card;
                this.cardName = cardName;
                this.modName = modName;
                this.weight = weight;
                this.hidden = hidden;
                this.allowMultiple = allowMultiple;
                this.canBeReassigned = canBeReassigned;
            }
        }

        public struct CardCategory
        {
            public readonly string name;

            public CardCategory(string name)
            {
                this.name = name;
            }

            private bool Equals(CardCategory cardCategory)
            {
                return cardCategory.name == this.name;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (obj is CardCategory cardCategory) 
                {
                    return this.Equals(cardCategory);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return name.GetHashCode();
            }

            public override string ToString()
            {
                return $"CardCategory: {this.name}";
            }
        }
    }
}
