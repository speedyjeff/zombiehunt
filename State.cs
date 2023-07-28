using System;
using System;

namespace ZombieHunt
{
    public class State
    {
        public Cards[][] Board { get; set; }
        public StatusDef Status { get; set; }
        public ErrorDef Error { get; set; }
        public PlayerColor Player { get; set; }
        public bool GameOver { get; set; }
        public bool EndPhase { get; set; }
        public int TurnsRemaining { get; set; } // only valid if EndPhase == True
        public int BrownScore { get; set; }
        public int BlueScore { get; set; }
        public int SelectedNumeric { get; set; }
        public int SelectedAlpha { get; set; }
    }
}
