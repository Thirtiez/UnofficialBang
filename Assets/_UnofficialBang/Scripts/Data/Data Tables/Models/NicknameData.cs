using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace Thirties.UnofficialBang
{
    [Serializable]
    public class NicknameData : BaseData
    {
        [OdinSerialize]
        [DisplayAsString]
        public string Nickname { get; set; }
    }
}
