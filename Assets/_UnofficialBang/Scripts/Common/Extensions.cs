using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public static class Extensions
    {
        public static T[] Shuffle<T>(this T[] array)
        {
            int count = array.Length;
            for (var i = 0; i < count - 1; ++i)
            {
                int random = UnityEngine.Random.Range(i, count);
                var temp = array[i];
                array[i] = array[random];
                array[random] = temp;
            }
            return array;
        }
    }
}
