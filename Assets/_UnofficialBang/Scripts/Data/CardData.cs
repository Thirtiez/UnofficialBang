using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace Thirties.UnofficialBang
{
    [Serializable]
    public class CardData
    {
        [OdinSerialize]
        [DisplayAsString]
        public int Id { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public string Name { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public string Sprite { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public CardClass Class { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public CardRank? Rank { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public CardSuit? Suit { get; set; }
    }
}
