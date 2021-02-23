using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        public void Configure(CardData cardData)
        {
            spriteRenderer.sprite = GameManager.Instance.CardSpriteDatabase.Resources.SingleOrDefault(c => c.name == cardData.Sprite);
        }
    }
}
