using System;

namespace ZombieHunt
{
    class Move
    {
        public int Priority { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }

        public static string Location(int numeric, int alpha)
        {
            return $"{Convert.ToChar((int)'A' + alpha)}{Convert.ToChar((int)'0' + numeric)}";
        }
    }
}
