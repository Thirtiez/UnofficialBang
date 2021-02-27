using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Sprite coveredCard;

        private CardData cardData;

        public void Configure(CardData cardData, bool isCovered)
        {
            this.cardData = cardData;

            spriteRenderer.sprite = isCovered ? coveredCard : GameManager.Instance?.CardSpriteTable?.Get(cardData.Sprite);
        }

        public void Reveal()
        {
            spriteRenderer.sprite = GameManager.Instance?.CardSpriteTable?.Get(cardData.Sprite);
        }

        public void Hide()
        {
            spriteRenderer.sprite = coveredCard;
        }
    }
}
