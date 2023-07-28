using System;

namespace ZombieHunt
{
    class Space
    {
        public Space(Cards both)
        {
            Showing = Card = both;
        }

        public Space(Cards showing, Cards card)
        {
            Showing = showing;
            Card = card;
        }

        public Cards Card { get; set; }		// the card on this space
        public Cards Showing { get; set; }	// the state of the showing card
        public int Numeric { get; set; }
        public int Alpha { get; set; }
    }
}
