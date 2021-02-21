using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thirties.UnofficialBang
{
    public static class Extensions
    {
        public static List<T> Shuffle<T>(this List<T> list)
        {
            int count = list.Count;
            for (var i = 0; i < count - 1; ++i)
            {
                int random = UnityEngine.Random.Range(i, count);
                var temp = list[i];
                list[i] = list[random];
                list[random] = temp;
            }
            return list;
        }

        public static PlayerRole? StringToPlayerRole(this string role)
        {
            switch (role)
            {
                case "Fuorilegge":
                    return PlayerRole.Outlaw;
                case "Sceriffo":
                    return PlayerRole.Sceriff;
                case "Vice":
                    return PlayerRole.Vice;
                case "Rinnegato":
                    return PlayerRole.Renegade;
            }

            return null;
        }
    }
}
