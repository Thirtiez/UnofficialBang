using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace Thirties.UnofficialBang
{
    [Serializable]
    public class PlayerCustomProperties
    {
        public int HandCount { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int BonusDistance { get; set; }
    }
}
