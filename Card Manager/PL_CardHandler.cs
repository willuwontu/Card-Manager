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
    public class PL_CardHandler : MonoBehaviourPunCallbacks
    {
        public Player player;
        public PL_Damagable pl_damagable;

        private void Awake()
        {
            this.player = GetComponent<Player>();
            this.pl_damagable = GetComponent<PL_Damagable>();
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            this.player.refs.view.RPC(nameof(PL_CardHandler.RPCA_SyncCards), RpcTarget.All, new object[]
                {
                    newPlayer.ActorNumber,
                    this.player.cards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray()
                });
        }

        [PunRPC]
        public void RPCA_SyncCards(int actorNumber, int[] cardIDs)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                RPCA_SetCards(cardIDs);
            }
        }

        [PunRPC]
        public void RPCA_RequestSync(int requestingActorNumber)
        {
            if (this.player.refs.view.IsMine)
            {
                this.player.refs.view.RPC(nameof(PL_CardHandler.RPCA_SyncCards), RpcTarget.All, new object[]
                { 
                    requestingActorNumber, 
                    this.player.cards.Select(c => CardHandler.instance.GetIDOfCard(c)).ToArray() 
                });
            }
        }

        [PunRPC]
        public void RPCA_RemoveCards()
        {
            player.RemoveCards();
        }

        [PunRPC]
        public void RPCA_SetCards(int[] cardIDs)
        {
            RPCA_RemoveCards();

            foreach (int cardID in cardIDs)
            {
                pl_damagable.RPCA_ADDCARDWITHID(cardID);
            }
        }

        [PunRPC]
        public void RPCA_UpdateCards(int[] cardIDs)
        {
            RPCA_RemoveCards();

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
