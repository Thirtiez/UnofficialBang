using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public static class FSMTrigger
    {
        // Common
        public static readonly int Forward = Animator.StringToHash("Forward");
        public static readonly int Back = Animator.StringToHash("Back");

        // Match
        public static readonly int Preparation = Animator.StringToHash("Preparation");
        public static readonly int Game = Animator.StringToHash("Game");
        public static readonly int End = Animator.StringToHash("End");

        // Preparation
        public static readonly int RolesDealing = Animator.StringToHash("RolesDealing");
        public static readonly int CharactersDealing = Animator.StringToHash("CharactersDealing");
        public static readonly int CardsDealing = Animator.StringToHash("CardsDealing");

        // Game
        public static readonly int TurnStart = Animator.StringToHash("TurnStart");
        public static readonly int DrawPhase = Animator.StringToHash("DrawPhase");
        public static readonly int PlayPhase = Animator.StringToHash("PlayPhase");
        public static readonly int DiscardPhase = Animator.StringToHash("DiscardPhase");
        public static readonly int TurnEnd = Animator.StringToHash("TurnEnd");

        // Play phase
        public static readonly int CardSelection = Animator.StringToHash("CardSelection");
        public static readonly int TargetSelection = Animator.StringToHash("TargetSelection");
        public static readonly int CardResolution = Animator.StringToHash("CardResolution");
    }

    public static class PlayerPrefsKey
    {
        public static readonly string File = "File";
    }
}
