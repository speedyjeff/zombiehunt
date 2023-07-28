using System;

namespace ZombieHunt
{
    class CardCollection
    {
        public CardCollection()
        {
            Cards = new List<Cards>();
            Rand = new Random();
        }

        public void Add(Cards card)
        {
            // add new item
            Cards.Add(card);

            if (Cards.Count == 1) return;

            // swap two items
            int i1 = Rand.Next() % Cards.Count;
            int i2 = Cards.Count - 1;
            while (i1 == i2)
            {
                i1 = Rand.Next() % Cards.Count;
            }
            Cards tmp = Cards[i1];
            Cards[i1] = Cards[i2];
            Cards[i2] = tmp;
        }

        public Cards Next()
        {
            Cards card = Cards[0];
            Cards.Remove(card);

            return card;
        }

        #region private
        private List<Cards> Cards;
        private Random Rand;
        #endregion
    }
}
