using System;

namespace ZombieHunt
{
    static class CardSpec
    {
        public static int NumberOfCards
        {
            get
            {
                return (int)Cards.Neutral_Nonmoving_Two + 1;
            }
        }

        public static Cards Card(int index)
        {
            return (Cards)index;
        }

        public static int Count(Cards card)
        {
            switch (card)
            {
                case Cards.Inaccessible:
                    return 0;
                case Cards.Blank:
                    return 0;
                case Cards.FaceDown:
                    return 0;
                case Cards.Blue_One:
                    return 2;
                case Cards.Blue_Two:
                    return 6;
                case Cards.Brown_One:
                    return 2;
                case Cards.Brown_Two_Top:
                    return 2;
                case Cards.Brown_Two_Bottom:
                    return 2;
                case Cards.Brown_Two_Left:
                    return 2;
                case Cards.Brown_Two_Right:
                    return 2;
                case Cards.Neutral_One:
                    return 8;
                case Cards.Neutral_Two:
                    return 7;
                case Cards.Neutral_Nonmoving_One:
                    return 8;
                case Cards.Neutral_Nonmoving_Two:
                    return 7;
                default:
                    throw new Exception($"Over the end of the enum: {card}");
            }
        }

        public static int Value(Cards card)
        {
            switch (card)
            {
                case Cards.Inaccessible:
                    return 0;
                case Cards.Blank:
                    return 0;
                case Cards.FaceDown:
                    return 0;
                case Cards.Blue_One:
                    return 10;
                case Cards.Blue_Two:
                    return 5;
                case Cards.Brown_One:
                    return 5;
                case Cards.Brown_Two_Top:
                    return 5;
                case Cards.Brown_Two_Bottom:
                    return 5;
                case Cards.Brown_Two_Left:
                    return 5;
                case Cards.Brown_Two_Right:
                    return 5;
                case Cards.Neutral_One:
                    return 3;
                case Cards.Neutral_Two:
                    return 2;
                case Cards.Neutral_Nonmoving_One:
                    return 2;
                case Cards.Neutral_Nonmoving_Two:
                    return 2;
                default:
                    throw new Exception($"Over the end of the enum: {card}");
            }
        }

        public static int Movement(Cards card)
        {
            switch (card)
            {
                case Cards.Inaccessible:
                    return 0;
                case Cards.Blank:
                    return 0;
                case Cards.FaceDown:
                    return 0;
                case Cards.Blue_One:
                    return 1;
                case Cards.Blue_Two:
                    return 10;
                case Cards.Brown_One:
                    return 1;
                case Cards.Brown_Two_Top:
                    return 10;
                case Cards.Brown_Two_Bottom:
                    return 10;
                case Cards.Brown_Two_Left:
                    return 10;
                case Cards.Brown_Two_Right:
                    return 10;
                case Cards.Neutral_One:
                    return 10;
                case Cards.Neutral_Two:
                    return 10;
                case Cards.Neutral_Nonmoving_One:
                    return 0;
                case Cards.Neutral_Nonmoving_Two:
                    return 0;
                default:
                    throw new Exception($"Over the end of the enum: {card}");
            }
        }

        public static bool ValidMove(Cards srcCard, Cards dstCard, int numericDelta, int alphaDelta)
        {
            return ValidMove(srcCard, dstCard, numericDelta, alphaDelta, out var wd);
        }

        public static bool ValidMove(Cards srcCard, Cards dstCard, int numericDelta, int alphaDelta, out bool wrongDirection)
        {
            // set if the attack was done in the wrong direction (only interesting when false is returned)
            wrongDirection = false;

            switch (dstCard)
            {
                case Cards.Inaccessible:
                    return false;
                case Cards.Blank:
                    return true;
                case Cards.FaceDown:
                    return false;
                case Cards.Blue_One:
                    // Only Brown_Two_* can attack
                    if (srcCard != Cards.Brown_Two_Top
                        && srcCard != Cards.Brown_Two_Bottom
                        && srcCard != Cards.Brown_Two_Right
                        && srcCard != Cards.Brown_Two_Left)
                    {
                        return false;
                    }

                    // must be the right direction
                    if ((numericDelta > 0 && srcCard == Cards.Brown_Two_Bottom)
                        || (numericDelta < 0 && srcCard == Cards.Brown_Two_Top)
                        || (alphaDelta > 0 && srcCard == Cards.Brown_Two_Right)
                        || (alphaDelta < 0 && srcCard == Cards.Brown_Two_Left))
                    {
                        return true;
                    }

                    // the attack is in the wrong direction
                    wrongDirection = true;
                    return false;
                case Cards.Blue_Two:
                    // Only Brown_Two_* can attack
                    if (srcCard != Cards.Brown_Two_Top
                        && srcCard != Cards.Brown_Two_Bottom
                        && srcCard != Cards.Brown_Two_Right
                        && srcCard != Cards.Brown_Two_Left)
                    {
                        return false;
                    }

                    // must be the right direction
                    if ((numericDelta > 0 && srcCard == Cards.Brown_Two_Bottom)
                        || (numericDelta < 0 && srcCard == Cards.Brown_Two_Top)
                        || (alphaDelta > 0 && srcCard == Cards.Brown_Two_Right)
                        || (alphaDelta < 0 && srcCard == Cards.Brown_Two_Left))
                    {
                        return true;
                    }

                    // the attack is in the wrong direction
                    wrongDirection = true;
                    return false;
                case Cards.Brown_One:
                    // only Blue_One can attack
                    return (srcCard == Cards.Blue_One);
                case Cards.Brown_Two_Top:
                    // only Blue_One can attack
                    return (srcCard == Cards.Blue_One);
                case Cards.Brown_Two_Bottom:
                    // only Blue_One can attack
                    return (srcCard == Cards.Blue_One);
                case Cards.Brown_Two_Left:
                    // only Blue_One can attack
                    return (srcCard == Cards.Blue_One);
                case Cards.Brown_Two_Right:
                    // only Blue_One can attack
                    return (srcCard == Cards.Blue_One);
                case Cards.Neutral_One:
                    // Blue_Two or Brown_Two_* can attack
                    if (srcCard == Cards.Blue_Two) return true;

                    if (srcCard != Cards.Brown_Two_Top
                        && srcCard != Cards.Brown_Two_Bottom
                        && srcCard != Cards.Brown_Two_Right
                        && srcCard != Cards.Brown_Two_Left)
                    {
                        return false;
                    }

                    // must be the right direction
                    if ((numericDelta > 0 && srcCard == Cards.Brown_Two_Bottom)
                        || (numericDelta < 0 && srcCard == Cards.Brown_Two_Top)
                        || (alphaDelta > 0 && srcCard == Cards.Brown_Two_Right)
                        || (alphaDelta < 0 && srcCard == Cards.Brown_Two_Left))
                    {
                        return true;
                    }

                    // the attack is in the wrong direction
                    wrongDirection = true;
                    return false;
                case Cards.Neutral_Two:
                    // Blue_Two or Brown_Two_* can attack
                    if (srcCard == Cards.Blue_Two) return true;

                    if (srcCard != Cards.Brown_Two_Top
                        && srcCard != Cards.Brown_Two_Bottom
                        && srcCard != Cards.Brown_Two_Right
                        && srcCard != Cards.Brown_Two_Left)
                    {
                        return false;
                    }

                    // must be the right direction
                    if ((numericDelta > 0 && srcCard == Cards.Brown_Two_Bottom)
                        || (numericDelta < 0 && srcCard == Cards.Brown_Two_Top)
                        || (alphaDelta > 0 && srcCard == Cards.Brown_Two_Right)
                        || (alphaDelta < 0 && srcCard == Cards.Brown_Two_Left))
                    {
                        return true;
                    }

                    // the attack is in the wrong direction
                    wrongDirection = true;
                    return false;
                case Cards.Neutral_Nonmoving_One:
                    // only Brown_One can attack
                    return (srcCard == Cards.Brown_One);
                case Cards.Neutral_Nonmoving_Two:
                    // only Brown_One can attack
                    return (srcCard == Cards.Brown_One);
                default:
                    return false;
            }
        }
    }
}
