﻿namespace Thirties.UnofficialBang
{
    public enum CardClass
    {
        Brown,
        Blue,
        Character,
        Role
    }

    public enum CardSuit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum CardRank
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    public enum CardTrigger
    {
        Passive,
        Played,
        PlayedAgainst,
        TurnStarted,
        Hurt,
        DrawPhaseStarted,
        CardSelectionStarted,
        Extracting,
        CharacterDied
    }

    public enum CardTarget
    {
        Self,
        Instigator,
        Range,
        FixedRange,
        Anyone,
        Everyone,
        EveryoneElse,
    }

    public enum CardEffect
    {
        Sceriff,
        Renegade,
        Outlaw,
        Barrel,
        Dynamite,
        Scope,
        Mustang,
        Prison,
        Weapon,
        Volcanic,
        Bang,
        Missed,
        GainHealth,
        DrawCard,
        DiscardCard,
        Duel,
        GeneralStore,
        Indians,
        BlackJack,
        CalamityJanet,
        JesseJones,
        KitCarlson,
        LuckyDuke,
        PedroRamirez,
        SidKetchum,
        SlabTheKiller,
        SuzyLaFayette,
        VultureSam
    }
}
