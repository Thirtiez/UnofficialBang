using Photon.Realtime;
using Sirenix.Serialization;
using System;

namespace Thirties.UnofficialBang
{
    [Serializable]
    public class RevealRoleEventData : BaseEventData
    {
        [OdinSerialize]
        public Player Player { get; set; }

        [OdinSerialize]
        public CardData Card { get; set; }
    }
}
