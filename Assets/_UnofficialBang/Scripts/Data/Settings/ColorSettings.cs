using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public enum TextColorization
    {
        BrownCard,
        BlueCard,
        CharacterCard,
        RoleCard,
        PlayerColor,
        DamageColor,
        CureColor,
    }

    [CreateAssetMenu(menuName = "BANG/Color Settings")]
    public class ColorSettings : SerializedScriptableObject
    {
        [Header("Text")]

        [SerializeField]
        private Color brownCardColor;

        [SerializeField]
        private Color blueCardColor;

        [SerializeField]
        private Color characterCardColor;

        [SerializeField]
        private Color roleCardColor;

        [SerializeField]
        private Color playerColor;

        [SerializeField]
        private Color damageColor;

        [SerializeField]
        private Color cureColor;

        [Header("Cards")]

        [SerializeField]
        private Color cardPlayable;

        [SerializeField]
        private Color cardReady;

        [Header("Areas")]

        [SerializeField]
        private Color areaPlayable;

        [SerializeField]
        private Color areaReady;

        [SerializeField]
        private Color areaTurn;

        public Color CardPlayable => cardPlayable;
        public Color CardReady => cardReady;
        public Color AreaPlayable => areaPlayable;
        public Color AreaReady => areaReady;
        public Color AreaTurn => areaTurn;

        public string Colorize(string text, TextColorization colorization)
        {
            var color = Color.white;
            switch (colorization)
            {
                case TextColorization.BrownCard:
                    color = brownCardColor;
                    break;
                case TextColorization.BlueCard:
                    color = blueCardColor;
                    break;
                case TextColorization.CharacterCard:
                    color = characterCardColor;
                    break;
                case TextColorization.RoleCard:
                    color = roleCardColor;
                    break;
                case TextColorization.PlayerColor:
                    color = playerColor;
                    break;
                case TextColorization.DamageColor:
                    color = damageColor;
                    break;
                case TextColorization.CureColor:
                    color = cureColor;
                    break;
            }

            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
        }
    }
}
