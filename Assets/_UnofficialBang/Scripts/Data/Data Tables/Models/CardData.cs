﻿using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace Thirties.UnofficialBang
{
    [Serializable]
    public class CardData : BaseData
    {
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

        [OdinSerialize]
        [DisplayAsString]
        public int? Health { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public CardTarget Target { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public CardEffect Effect { get; set; }

        [OdinSerialize]
        [DisplayAsString]
        public int? EffectValue { get; set; }
    }
}
