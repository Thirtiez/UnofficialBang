using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Thirties.UnofficialBang
{
    public class CardElementUI : MonoBehaviour
    {
        [SerializeField]
        private Button cardButton;

        [SerializeField]
        private Image cardImage;

        [SerializeField]
        private Sprite cardBack;

        public void Configure(int cardId, bool isCovered)
        {
            if (isCovered)
            {
                cardImage.sprite = cardBack;
            }
            else
            {
                var cardData = GameManager.Instance.Cards[cardId];
                cardImage.sprite = GameManager.Instance.CardSpriteTable.Get(cardData.Sprite);
            }

            cardButton.onClick.AddListener(() => GameManager.Instance.CardPickerExit?.Invoke(new CardPickerExitEventData { CardId = cardId, IsFromHand = isCovered}));
        }
    }
}
