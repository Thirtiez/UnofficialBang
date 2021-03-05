using Photon.Pun;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardSelectionState : PlayPhaseState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (_gameManager.IsLocalPlayerTurn)
            {
                var character = _gameManager.Cards[PhotonNetwork.LocalPlayer.CharacterCardId];
                switch (character.Effect)
                {
                    case CardEffect.CalamityJanet:
                        //TODO CalamityJanet
                        break;
                    case CardEffect.SidKetchum:
                        //TODO SidKetchum
                        break;
                    case CardEffect.SuzyLaFayette:
                        //TODO SuzyLaFayette
                        break;
                        break;
                }
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
        }
    }
}