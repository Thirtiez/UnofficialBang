using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private Animator fsm;

        public static GameManager Instance { get; private set; }

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
    }
}

