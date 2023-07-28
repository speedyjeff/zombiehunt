using System;

namespace ZombieHunt
{
    // Rules:
    //   2 players, they take turns
    //   On each turn you can either flip a card or move a card
    //
    //    1. Flipping over a face down card
    //       Any card that is face down may be flipped over (the orientation will not change)
    //    2. Move a card
    //       a. Blue player may move both Blue and Neutral
    //       b. Brown player may move both Brown and Neutral
    //       c. No player may move Neutral_Nonmoving cards
    //       d. Cards may only move in straight lines over cleared paths (Blank)
    //       e. Cards can only be moved the number of spaces permitted
    //          i.   Blue_One    = 1
    //          ii.  Blue_Two    = unlimited
    //          iii. Brown_One   = 1
    //          iv.  Brown_Two   = unlimited
    //          v.   Neutral_One = unlimited
    //          vi.  Neutral_Two = unlimited
    //       f. No 'null' moves (must move)
    //       g. Cannot move a card back to a place just moved by the other player (or yourself)
    //       h. Each tile has a point value
    //          i.   Blue_One         = 10
    //          ii.  Blue_Two         = 5
    //          iii. Brown_One        = 5
    //          iv.  Brown_Two        = 5
    //          v.   Neutral_One      = 3
    //          vi.  Neutral_Two      = 2
    //          vii. Neutral_Unmoving = 2
    //       i. There are rules on who can attack who
    //          Blue_One    - Brown_One, Brown_Two_*
    //          Blue_Two    - Neutral_One, Neutral_Two
    //          Brown_One   - Neutral_Nonmoving_*
    //          Brown_Two_* - Blue_One, Blue_Two, Neutral_One, Neutral_Two (but only in the direction that Brown_Two is pointing)
    //          Neutral_One - <empty>
    //          Neutral_Two - <empty>
    //    3. Once all the tiles are flipped, the 'end-phase' begins
    //       5 turns remain
    //       Pieces may be moved off the board to add to their own points
    //
    //    Game ends IFF:
    //       A player has no colors of their own, or if Neutrals are not movable, or if cannot move their colored pieces
    //
    // State machine
    //
    //                              --------------------------valid------------------------
    //                              |                                                     |
    //                              |                    ---------invalid---------------  |
    //                             \|/                  \|/                            |  |
    //    |Start_Game| ------> |Next_Player| ----> |Select_None| --> |Flip_Card| ---------|
    //        /|\                   |                    |                             |  |
    //         |                   \|/                  \|/                            |  |
    //         --------------- |Game_Over|        |Select_One| ------------------------|  |
    //                                                   |             ----------------|  |
    //                                                   |             |                  |
    //                                                   |-------- |Move_Card| -----------|

    enum GameState { Start_Game, Next_Player, Select_None, Flip_Card, Select_One, Move_Card, Game_Over };

    public class ZombieHuntGame
    {
        public ZombieHuntGame()
        {
            Init();
        }

        // public methods for game play
        public void Start()
        {
            EnsureState(GameState.Start_Game);

            // setup for the first player
            FirstPlayer();
        }

        public void ReStart()
        {
            EnsureState(GameState.Game_Over);

            // re-initialize the data
            Init();

            // setup for the first player
            FirstPlayer();
        }

        public bool SelectCard(string index)
        {
            EnsureState(GameState.Select_None, GameState.Select_One);

            // clear out error status
            Error = ErrorDef.None;

            // grab the space
            var space = FindSpace(index);
            if (space == null) return false;

            // main game logic
            if (State == GameState.Select_None)
            {
                // first selection
                switch (space.Showing)
                {
                    case Cards.FaceDown:
                        if (FlipCard(space))
                        {
                            // change players
                            AdvancePlayer();
                            return true;
                        }

                        throw new Exception("Failed to flip card " + space.Card + " for unknown reason");
                    case Cards.Inaccessible: Error = ErrorDef.NotSelectable; return false;
                    case Cards.Blank: Error = ErrorDef.NotSelectable; return false;
                    case Cards.Neutral_Nonmoving_One: Error = ErrorDef.NotSelectable; return false;
                    case Cards.Neutral_Nonmoving_Two: Error = ErrorDef.NotSelectable; return false;
                    default:
                        {
                            if (SelectCard(space))
                            {
                                Status = StatusDef.SelectSecond;
                                return true;
                            }

                            // start over
                            ClearSelection();
                            return false;
                        }
                }
            }
            else
            {
                // second selection
                switch (space.Showing)
                {
                    case Cards.FaceDown: Error = ErrorDef.NotSelectable; ClearSelection(); return false;
                    case Cards.Inaccessible: Error = ErrorDef.NotSelectable; ClearSelection(); return false;
                    default:
                        {
                            if (MoveCard(space)) return true;

                            // need to go back and start the selection over
                            ClearSelection();
                            return false;
                        }
                }
            }
        }

        public State CurrentState
        {
            get
            {
                State newState = new State();

                // initialize 
                newState.Board = new Cards[cHeight][];
                for(int i=0; i<newState.Board.Length; i++) newState.Board[i] = new Cards[cWidth];
                newState.Status = Status;
                newState.Error = Error;
                newState.Player = Player;
                newState.BlueScore = Points[(int)PlayerColor.Blue];
                newState.BrownScore = Points[(int)PlayerColor.Brown];
                newState.SelectedNumeric = -1;
                newState.SelectedAlpha = -1;
                if (SelectedSpace != null)
                {
                    newState.SelectedNumeric = SelectedSpace.Numeric;
                    newState.SelectedAlpha = SelectedSpace.Alpha;
                }

                if (TurnsRemaining >= 0)
                {
                    newState.EndPhase = true;
                    newState.TurnsRemaining = TurnsRemaining;
                }

                newState.GameOver = (State == GameState.Game_Over);

                // copy board
                for (int numeric = 0; numeric < Board.Length; numeric++)
                {
                    for (int alpha = 0; alpha < Board[numeric].Length; alpha++)
                    {
                        newState.Board[numeric][alpha] = Board[numeric][alpha].Showing;
                    }
                }

                return newState;
            }
        }

        #region private
        private const int cHeight = 9;
        private const int cWidth = 9;
        private const int cEndPhaseTurns = 5;
        private Space[][] Board;
        private GameState State;
        private PlayerColor Player;
        private Space SelectedSpace;
        private int[] Points;
        private StatusDef Status;
        private ErrorDef Error;
        private int TurnsRemaining;

        ////////////////////////////////////////////////////////////////////////////////////
        // All state changes happen in this region
        //
        private void Init()
        {
            // initialize
            var rand = new Random();
            Board = new Space[cHeight][];
            for (int i = 0; i < Board.Length; i++) Board[i] = new Space[cWidth];
            var cards = new CardCollection();
            State = GameState.Start_Game;
            Player = PlayerColor.None;
            SelectedSpace = null;
            Points = new int[(int)PlayerColor.Brown + 1];
            Points[(int)PlayerColor.Blue] = 0;
            Points[(int)PlayerColor.Brown] = 0;
            Status = StatusDef.None;
            Error = ErrorDef.None;
            TurnsRemaining = -1;

            // populate the collection
            for (int i = 0; i < CardSpec.NumberOfCards; i++)
            {
                Cards card = CardSpec.Card(i);

                for (int j = 0; j < CardSpec.Count(card); j++) cards.Add(card);
            }

            // set the board
            for (int numeric = 0; numeric < Board.Length; numeric++)
            {
                for (int alpha = 0; alpha < Board[numeric].Length; alpha++)
                {

                    if ((numeric == 0 && alpha == cWidth / 2)
                        ||
                        (numeric == cHeight / 2 && alpha == 0)
                        ||
                        (numeric == cHeight / 2 && alpha == cWidth - 1)
                        ||
                        (numeric == cHeight - 1 && alpha == cWidth / 2)
                        ||
                        (numeric == (cHeight / 2) && alpha == (cWidth / 2))
                        )
                    {
                        // special entrances and center are blank
                         Board[numeric][alpha] = new Space(Cards.Blank);
                    }
                    else if (numeric == 0
                        || alpha == 0
                        || numeric + 1 == cHeight
                        || alpha + 1 == cWidth
                    )
                    {
                        // border's are inaccessible
                        Board[numeric][alpha] = new Space(Cards.Inaccessible);
                    }
                    else
                    {
                        // all others are spaces
                        Board[numeric][alpha] = new Space(Cards.FaceDown, cards.Next());
                    }

                    // set the location
                    Board[numeric][alpha].Numeric = numeric;
                    Board[numeric][alpha].Alpha = alpha;
                }
            }
        }

        private void FirstPlayer()
        {
            ChangeState(GameState.Next_Player);
            Player = PlayerColor.Blue;
            Status = StatusDef.SelectFirst;
            ChangeState(GameState.Select_None);
        }

        private void AdvancePlayer()
        {
            ChangeState(GameState.Next_Player);

            bool endPhase = true;
            int browns = 0;
            int blues = 0;
            int notMoveBlues = 0;
            int notMoveBrowns = 0;

            // collect data
            for (int numeric = 0; numeric < Board.Length; numeric++)
            {
                for (int alpha = 0; alpha < Board[numeric].Length; alpha++)
                {
                    // endPhase when all cards are flipped up
                    if (Board[numeric][alpha].Showing == Cards.FaceDown) endPhase = false;
                    // check for the status of Blue's
                    if (Board[numeric][alpha].Card == Cards.Blue_One
                        || Board[numeric][alpha].Card == Cards.Blue_Two)
                    {
                        blues++;
                        if (!CanMove(Board[numeric][alpha])) notMoveBlues++;
                    }
                    // check for the status of Brown's
                    if (Board[numeric][alpha].Card == Cards.Brown_One
                        || Board[numeric][alpha].Card == Cards.Brown_Two_Bottom
                        || Board[numeric][alpha].Card == Cards.Brown_Two_Top
                        || Board[numeric][alpha].Card == Cards.Brown_Two_Left
                        || Board[numeric][alpha].Card == Cards.Brown_Two_Right
                        )
                    {
                        browns++;
                        if (!CanMove(Board[numeric][alpha])) notMoveBrowns++;
                    }
                }
            }

            // check for end phase (if all cards are showing) and decrement turnsRemaining if in EndPhase
            if (endPhase && TurnsRemaining < 0) TurnsRemaining = cEndPhaseTurns;
            else if (endPhase) TurnsRemaining--;

            // check for game over
            //  Game over can occur IFF
            //   In EndPhase and the last turn has expired
            //   OR
            //   A player's colors are all gone
            //   OR
            //   A player cannot move any of its pieces
            if (TurnsRemaining == 0
                || (blues == 0 || browns == 0)
                || (blues == notMoveBlues || browns == notMoveBrowns))
            {
                // game over
                ChangeState(GameState.Game_Over);
                Status = StatusDef.GameOver;
            }
            else
            {
                // advance the player
                if (Player == PlayerColor.Blue) Player = PlayerColor.Brown;
                else if (Player == PlayerColor.Brown) Player = PlayerColor.Blue;

                // set the correct status
                if (TurnsRemaining > 0)
                    Status = StatusDef.EndPhaseSelectFirst;
                else
                    Status = StatusDef.SelectFirst;

                ChangeState(GameState.Select_None);
            }
        }

        private bool FlipCard(Space card)
        {
            ChangeState(GameState.Flip_Card);

            // check that this is valid
            if (card.Showing != Cards.FaceDown) throw new Exception("Trying to flip a card that is not face down!");
            if (card.Card == Cards.FaceDown
                || card.Card == Cards.Inaccessible
                || card.Card == Cards.Blank) throw new Exception("Trying to flip a card that has invalid card type: " + card.Card);

            // flip the card
            card.Showing = card.Card;

            return true;
        }

        private bool SelectCard(Space card)
        {
            ChangeState(GameState.Select_One);

            // Card must be moveable
            if (card.Showing == Cards.Blank
                || card.Showing == Cards.FaceDown
                || card.Showing == Cards.Inaccessible
                || card.Showing == Cards.Neutral_Nonmoving_One
                || card.Showing == Cards.Neutral_Nonmoving_Two) throw new Exception("Invalid card selection: " + card.Showing);

            // Card must be of same color or neutral
            if ((Player == PlayerColor.Blue
                && card.Showing != Cards.Blue_One
                && card.Showing != Cards.Blue_Two
                && card.Showing != Cards.Neutral_One
                && card.Showing != Cards.Neutral_Two)
                ||
                (Player == PlayerColor.Brown
                && card.Showing != Cards.Brown_One
                && card.Showing != Cards.Brown_Two_Bottom
                && card.Showing != Cards.Brown_Two_Left
                && card.Showing != Cards.Brown_Two_Right
                && card.Showing != Cards.Brown_Two_Top
                && card.Showing != Cards.Neutral_One
                && card.Showing != Cards.Neutral_Two)
                )
            {
                Error = ErrorDef.NonPlayerCard;
                return false;
            }

            // store the selected card
            SelectedSpace = card;

            return true;
        }

        private bool MoveCard(Space card)
        {
            ChangeState(GameState.Move_Card);

            // TODO: Check for moving back and forth (by other player, or same player)

            // do some sanity checking
            if (SelectedSpace == null) throw new Exception("Previous selection is null!");

            // calculations
            var numericDelta = card.Numeric - SelectedSpace.Numeric;
            var alphaDelta = card.Alpha - SelectedSpace.Alpha;

            // ensure that the move is not diagonal
            if (Math.Abs(numericDelta) > 0 && Math.Abs(alphaDelta) > 0)
            {
                Error = ErrorDef.InvalidDiagonalMove;
                return false;
            }

            // ensure there are only blanks exist in-between
            if (alphaDelta == 0) // the Alpha's are the same
            {
                for (int numeric = Math.Min(card.Numeric, SelectedSpace.Numeric) + 1; numeric < Math.Max(card.Numeric, SelectedSpace.Numeric); numeric++)
                {
                    if (Board[numeric][card.Alpha].Showing != Cards.Blank)
                    {
                        Error = ErrorDef.NonClearPath;
                        return false;
                    }
                }
            }
            else
            {
                for (int alpha = Math.Min(card.Alpha, SelectedSpace.Alpha) + 1; alpha < Math.Max(card.Alpha, SelectedSpace.Alpha); alpha++)
                {
                    if (Board[card.Numeric][alpha].Showing != Cards.Blank)
                    {
                        Error = ErrorDef.NonClearPath;
                        return false;
                    }
                }
            }

            // ensure the move is within the allocated movements
            var distance = Math.Abs(numericDelta) + Math.Abs(alphaDelta); // one of these is zero
            if (distance > CardSpec.Movement(SelectedSpace.Showing))
            {
                Error = ErrorDef.ExceedMaxMove;
                return false;
            }

            // validate attack/move
            if (!CardSpec.ValidMove(SelectedSpace.Showing, card.Showing, numericDelta, alphaDelta, out var wd))
            {
                if (wd) Error = ErrorDef.InvalidAttackDirection;
                else Error = ErrorDef.InvalidAttack;

                return false;
            }

            // increment the score
            Points[(int)Player] += CardSpec.Value(card.Showing);

            // make the move
            card.Card = SelectedSpace.Card;
            card.Showing = SelectedSpace.Showing;

            // make the vacated space Blank
            SelectedSpace.Card = Cards.Blank;
            SelectedSpace.Showing = Cards.Blank;

            // clear out the selected space
            SelectedSpace = null;

            // if in EndPhase, if the player moves to the end points they are awarded the points
            if (TurnsRemaining > 0
                &&
                (
                (card.Numeric == 0 && card.Alpha == (cWidth / 2))
                ||
                (card.Numeric == cHeight - 1 && card.Alpha == (cWidth / 2))
                ||
                (card.Numeric == (cHeight / 2) && card.Alpha == 0)
                ||
                (card.Numeric == (cHeight / 2) && card.Alpha == cWidth - 1)
                )
                )
            {
                // card is now updated to the moved piece
                Points[(int)Player] += CardSpec.Value(card.Showing);

                // clear it off the board
                card.Card = Cards.Blank;
                card.Showing = Cards.Blank;
            }

            // go to next player
            AdvancePlayer();

            return true;
        }

        private void ClearSelection()
        {
            ChangeState(GameState.Select_None);
            Status = StatusDef.SelectFirst;
            SelectedSpace = null;
        }

        private Space FindSpace(string index)
        {
            int numeric = -1;
            int alpha = -1;

            // A valid index consists of a numeric value (0-8) and a alpha (a-i) (in either order: ex: 0A or A0)

            if (index.Length != 2)
            {
                Error = ErrorDef.InvalidCard;
                return null;
            }

            for (int i = 0; i < 2; i++)
            {
                if (index[i] >= 'a' && index[i] <= 'i') alpha = index[i] - 'a';
                if (index[i] >= 'A' && index[i] <= 'I') alpha = index[i] - 'A';
                if (index[i] >= '0' && index[i] <= '9') numeric = index[i] - '0';
            }

            if (numeric >= 0 && alpha >= 0)
            {
                return Board[numeric][alpha];
            }

            Error = ErrorDef.InvalidCard;
            return null;
        }

        private bool CanMove(Space space)
        {
            if (space.Showing == Cards.FaceDown)
            {
                // if a space is not flipped yet, it can still move
                return true;
            }
            else if (space.Showing == Cards.Neutral_Nonmoving_One
                || space.Showing == Cards.Neutral_Nonmoving_Two)
            {
                // these cards cannot move
                return false;
            }

            // check the 4 possible moves and see if they can move
            bool wd = false;
            if (space.Numeric > 0 && CardSpec.ValidMove(space.Showing, Board[space.Numeric - 1][space.Alpha].Showing, -1, 0, out wd)) return true;
            if (space.Numeric < (cHeight - 1) && CardSpec.ValidMove(space.Showing, Board[space.Numeric + 1][space.Alpha].Showing, 1, 0, out wd)) return true;
            if (space.Alpha > 0 && CardSpec.ValidMove(space.Showing, Board[space.Numeric][space.Alpha - 1].Showing, 0, -1, out wd)) return true;
            if (space.Alpha < (cWidth - 1) && CardSpec.ValidMove(space.Showing, Board[space.Numeric][space.Alpha + 1].Showing, 0, 1, out wd)) return true;

            return false;
        }

        // internal state management
        private void EnsureState(GameState vState)
        {
            if (State != vState) throw new Exception("Expected to be in state " + vState + " but was in " + State + " instead");
        }

        private void EnsureState(GameState v1State, GameState v2State)
        {
            if (State != v1State
                && State != v2State) throw new Exception("Expected to be in state " + v1State + " (or " + v2State + ") but was in " + State + " instead");
        }

        private void EnsureState(GameState v1State, GameState v2State, GameState v3State)
        {
            if (State != v1State
                && State != v2State
                && State != v3State) throw new Exception("Expected to be in state " + v1State + " (or " + v2State + " or " + v3State + ") but was in " + State + " instead");
        }

        // only some transitions are allowed, ensure proper transition
        private void ChangeState(GameState newState)
        {
            bool invalidStateChange = false;

            switch (State)
            {
                case GameState.Start_Game:
                    if (newState != GameState.Next_Player) invalidStateChange = true;
                    break;
                case GameState.Next_Player:
                    if (newState != GameState.Select_None
                    && newState != GameState.Game_Over) invalidStateChange = true;
                    break;
                case GameState.Select_None:
                    if (newState != GameState.Flip_Card
                        && newState != GameState.Select_One) invalidStateChange = true;
                    break;
                case GameState.Select_One:
                    if (newState != GameState.Select_None
                    && newState != GameState.Move_Card) invalidStateChange = true;
                    break;
                case GameState.Flip_Card:
                    if (newState != GameState.Next_Player) invalidStateChange = true;
                    break;
                case GameState.Move_Card:
                    if (newState != GameState.Next_Player
                        && newState != GameState.Select_None) invalidStateChange = true;
                    break;
                case GameState.Game_Over:
                    if (newState != GameState.Start_Game) invalidStateChange = true;
                    break;
            }

            if (invalidStateChange) throw new Exception("Invalid transition from " + State + " to " + newState);

            // change state
            State = newState;
        }
        #endregion
    }
}
