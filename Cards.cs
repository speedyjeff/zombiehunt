using System;

namespace ZombieHunt
{
    public enum Cards
    {
        Inaccessible = 0,
        Blank = 1,
        FaceDown = 2,
        Blue_One = 3,	// Bear, Hero 
        Blue_Two = 4,	// Fox, Vigilantly
        Brown_One = 5,	// Logger, rats
        Brown_Two_Top = 6,	// Hunter, Zombie
        Brown_Two_Bottom = 7,
        Brown_Two_Right = 8,
        Brown_Two_Left = 9,
        Neutral_One = 10,	// Pheasant, Nerd
        Neutral_Two = 11,	// Duck, Woman
        Neutral_Nonmoving_One = 12,	// Fir, corpse #1
        Neutral_Nonmoving_Two = 13	// Oak, corpse #2
    }
}
