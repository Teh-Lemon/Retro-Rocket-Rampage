using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeckoFactionRRR
{
    public static class GameState
    {
        // List of game states
        public enum States
        {
            START,
            MENU,
            MENU_TIMEOUT,
            START_PLAYING,
            PLAYING,
            RACE_OVER,
            NEW_RACE,
            VICTORY_LAP,
            PRE_VICTORY_LAP,
            PREVIEW_TRACK,
            OVERALL_WINNER,
            PRE_MENU_TIMEOUT,
            PRE_START_SCREEN,
            START_SCREEN
        }

        // Current state
        public static States Current { get; set; }

        public static void Initialize(States startState)
        {
            Current = startState;
        }

    }
}
