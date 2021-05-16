using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    [CreateAssetMenu(menuName = "BANG/Animation Settings")]
    public class AnimationSettings : SerializedScriptableObject
    {
        [SerializeField]
        [SuffixLabel("s")]
        private float dealCardDelay = 0.2f;

        [SerializeField]
        [SuffixLabel("s")]
        private float dealCardDuration = 1.0f;

        [SerializeField]
        [SuffixLabel("s")]
        private float roleRevealDelay = 1.0f;

        [SerializeField]
        [SuffixLabel("s")]
        private float bulletAnimationDelay = 0.4f;

        [SerializeField]
        [SuffixLabel("s")]
        private float bulletAnimationDuration = 0.5f;

        public float DealCardDelay => dealCardDelay;
        public float DealCardDuration => dealCardDuration;
        public float RoleRevealDelay => roleRevealDelay;
        public float BulletAnimationDelay => bulletAnimationDelay;
        public float BulletAnimationDuration => bulletAnimationDuration;
    }
}
