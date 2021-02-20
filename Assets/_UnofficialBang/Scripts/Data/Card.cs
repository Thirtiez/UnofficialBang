using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sirenix.OdinInspector;
using System;

namespace Thirties.UnofficialBang
{
    [Serializable]
    public class Card
    {
        [ShowInInspector]
        [DisplayAsString]
        public string Id { get; set; }

        [ShowInInspector]
        [DisplayAsString]
        public string Name { get; set; }

        [ShowInInspector]
        [DisplayAsString]
        public string Sprite { get; set; }

        [ShowInInspector]
        [DisplayAsString]
        public CardClass? Class { get; set; }

        [ShowInInspector]
        [DisplayAsString]
        public CardRank? Rank { get; set; }

        [ShowInInspector]
        [DisplayAsString]
        public CardSuit? Suit { get; set; }
    }
}
