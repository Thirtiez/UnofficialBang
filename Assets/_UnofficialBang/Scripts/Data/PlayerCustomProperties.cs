using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace Thirties.UnofficialBang
{
    [Serializable]
    public class PlayerCustomProperties
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int DistanceModifier { get; set; }
        public int HandCount { get; set; }
    }
}
