using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Thirties.UnofficialBang
{
    public class CardElementUI : MonoBehaviour
    {
        [SerializeField]
        private Button cardButton;

        [SerializeField]
        private Image cardImage;

        public void Configure(Sprite cardSprite, UnityAction onButtonClick)
        {
            cardImage.sprite = cardSprite;
            cardButton.onClick.AddListener(onButtonClick);
        }
    }
}
