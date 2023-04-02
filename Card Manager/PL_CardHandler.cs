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

namespace R3DCore
{
    public class PL_CardHandler : MonoBehaviour
    {
        public Player player;
        public PL_Damagable pl_damagable;

        private void Awake()
        {
            this.player = GetComponent<Player>();
            this.pl_damagable = GetComponent<PL_Damagable>();
        }

        [PunRPC]
        public void RPCA_RemoveCards()
        {
            player.RemoveCards();
        }

        [PunRPC]
        public void RPCA_UpdateCards(int[] cardIDs)
        {
            foreach (CardUpgrade card in player.cards)
            {
                CardManager.OnCardRemovedFromPlayer(player, card);
            }

            player.RemoveCards();

            foreach (int cardID in cardIDs)
            {
                if (CardManager.GetCardInfoFromCard(CardHandler.instance.GetCardWithID(cardID)).canBeReassigned)
                {
                    pl_damagable.RPCA_ADDCARDWITHID(cardID);
                }
                else
                {
                    CardManager.OnReapplyCardToPlayer(player, CardHandler.instance.GetCardWithID(cardID));
                }
            }
        }
    }
}
