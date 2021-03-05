using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Thirties.UnofficialBang
{
    public class CommandElementUI : MonoBehaviour
    {
        [SerializeField]
        private Button commandButton;

        [SerializeField]
        private TMP_Text commandText;

        public void Configure(string content, UnityAction onButtonClick)
        {
            commandText.text = content;
            commandButton.onClick.AddListener(onButtonClick);
        }
    }
}
