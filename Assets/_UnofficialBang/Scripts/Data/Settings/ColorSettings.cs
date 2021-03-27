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

        [SerializeField]
        private Color damageColor;

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

        public string BrownCardColor => ColorUtility.ToHtmlStringRGBA(brownCardColor);
        public string BlueCardColor => ColorUtility.ToHtmlStringRGBA(blueCardColor);
        public string CharacterCardColor => ColorUtility.ToHtmlStringRGBA(characterCardColor);
        public string RoleCardColor => ColorUtility.ToHtmlStringRGBA(roleCardColor);
        public string PlayerColor => ColorUtility.ToHtmlStringRGBA(playerColor);
        public string DamageColor => ColorUtility.ToHtmlStringRGBA(damageColor);

        public Color CardPlayable => cardPlayable;
        public Color CardReady => cardReady;
        public Color AreaPlayable => areaPlayable;
        public Color AreaReady => areaReady;
        public Color AreaTurn => areaTurn;
    }
}
