using OneP.InfinityScrollView;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class LogElement : InfinityBaseItem
    {
        [SerializeField]
        private TMP_Text contentText;

        public override void Reload(InfinityScrollView _infinity, int _index)
        {
            base.Reload(_infinity, _index);

            var gameLog = _infinity.GetComponent<GameLog>();

            contentText.text = gameLog.Messages[_index];
        }
    }
}