using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class LogElementUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text contentText;

        public void Configure(string message)
        {
            contentText.text = message;
        }
    }
}