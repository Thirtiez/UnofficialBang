using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    [CreateAssetMenu(menuName = "Color Settings")]
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

        [Header("Cards")]
        [SerializeField]
        private Color cardHighlight;

        public string BrownCardColor => ColorUtility.ToHtmlStringRGBA(brownCardColor);
        public string BlueCardColor => ColorUtility.ToHtmlStringRGBA(blueCardColor);
        public string CharacterCardColor => ColorUtility.ToHtmlStringRGBA(characterCardColor);
        public string RoleCardColor => ColorUtility.ToHtmlStringRGBA(roleCardColor);
        public string PlayerColor => ColorUtility.ToHtmlStringRGBA(playerColor);

        public Color CardHighlight => cardHighlight;
    }
}
