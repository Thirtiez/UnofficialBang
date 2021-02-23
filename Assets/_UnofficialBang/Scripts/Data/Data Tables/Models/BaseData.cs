using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace Thirties.UnofficialBang
{
    public class BaseData
    {
        [OdinSerialize]
        [DisplayAsString]
        public int Id { get; set; }
    }
}
