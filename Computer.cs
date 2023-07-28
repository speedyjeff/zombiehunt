using System;

namespace ZombieHunt
{
    public static class Computer
    {
        public static string[] Selection(PlayerColor color, State state)
        {
            // either going to return one card (flip) or 2 cards (move)

            // basic validation
            if (color != state.Player) throw new Exception("Asked computer to play during other player's turn");

            // iterate through all the cards on the board and assign
            //  a priority to the card, and the possible move space
            var moves = new List<Move>();
            for (int numeric = 0; numeric < state.Board.Length; numeric++)
            {
                for (int alpha = 0; alpha < state.Board[numeric].Length; alpha++)
                {
                    // flip a card
                    if (state.Board[numeric][alpha] == Cards.FaceDown)
                    {
                        // priority for the space
                        var priority = PSourceRule(numeric, alpha);

                        // priority based on neighbors
                        if (numeric > 0) priority += PNextTooRule(color, state.Board[numeric][alpha], state.Board[numeric - 1][alpha], numericDelta: -1, alphaDelta: 0);
                        if (numeric < (state.Board.Length - 1)) priority += PNextTooRule(color, state.Board[numeric][alpha], state.Board[numeric + 1][alpha], numericDelta: 1, alphaDelta: 0);
                        if (alpha > 0) priority += PNextTooRule(color, state.Board[numeric][alpha], state.Board[numeric][alpha - 1], numericDelta: 0, alphaDelta: -1);
                        if (alpha < (state.Board[numeric].Length - 1)) priority += PNextTooRule(color, state.Board[numeric][alpha], state.Board[numeric][alpha + 1], numericDelta: 0, alphaDelta: 1);

                        // add the card to flip
                        moves.Add( new Move()
                        {
                            Source = Move.Location(numeric, alpha),
                            Destination = null,
                            Priority = priority
                        });
                    }

                    // this is our piece
                    else if (IsMine(color, state.Board[numeric][alpha]))
                    {
                        // priority based on movements
                        // move down until not allowed
                        for (int numericDelta = 1;
                            numericDelta <= CardSpec.Movement(state.Board[numeric][alpha])
                            && (numeric + numericDelta) < state.Board.Length;
                            numericDelta++)
                        {
                            var move = MovePriority(state.Board, state.EndPhase, numeric, alpha, numericDelta, 0);
                            if (move == null) break;
                            moves.Add(move);
                            if (state.Board[numeric + numericDelta][alpha] != Cards.Blank) break;
                        }

                        // move up until not allowed
                        for (int numericDelta = -1;
                            (numericDelta * -1) <= CardSpec.Movement(state.Board[numeric][alpha])
                            && (numeric + numericDelta) >= 0;
                            numericDelta--)
                        {
                            var move = MovePriority(state.Board, state.EndPhase, numeric, alpha, numericDelta, 0);
                            if (move == null) break;
                            moves.Add(move);
                            if (state.Board[numeric + numericDelta][alpha] != Cards.Blank) break;
                        }

                        // move right until not allowed
                        for (int alphaDelta = 1;
                            alphaDelta <= CardSpec.Movement(state.Board[numeric][alpha])
                            && (alpha + alphaDelta) < state.Board[numeric].Length;
                            alphaDelta++)
                        {
                            var move = MovePriority(state.Board, state.EndPhase, numeric, alpha, 0, alphaDelta);
                            if (move == null) break;
                            moves.Add(move);
                            if (state.Board[numeric][alpha + alphaDelta] != Cards.Blank) break;
                        }

                        // move right until not allowed
                        for (int alphaDelta = -1;
                            (alphaDelta * -1) <= CardSpec.Movement(state.Board[numeric][alpha])
                            && (alpha + alphaDelta) >= 0;
                            alphaDelta--)
                        {
                            var move = MovePriority(state.Board, state.EndPhase, numeric, alpha, 0, alphaDelta);
                            if (move == null) break;
                            moves.Add(move);
                            if (state.Board[numeric][alpha + alphaDelta] != Cards.Blank) break;
                        }
                    }
                }
            }

            // list contains all possible moves
            if (moves.Count == 0) throw new Exception("Failed to find a valid move");

            // find the max priority moves (there are likely many in that priority)
            var priorityMoves = new List<Move>();
            var maxPriority = Int32.MinValue;
            Move lastFlip = null;
            foreach (Move move in moves)
            {
                // find the highest priority
                if (move.Priority > maxPriority)
                {
                    maxPriority = move.Priority;
                    priorityMoves.Clear();
                }

                // capture all the high pri moves
                if (move.Priority == maxPriority)
                {
                    priorityMoves.Add(move);
                }

                // find a card to flip
                if (move.Destination == null) lastFlip = move;
            }

            // priorityMoves contains the best move possible
            if (priorityMoves.Count == 0) throw new Exception("Failed to get a list of priority moves");

            // select one of the moves
            Move priorityMove = null;
            if (maxPriority <= 0 && lastFlip != null)
            {
                // flip a card if there are no good moves left
                priorityMove = lastFlip;
            }
            else
            {
                // grab one of the possible high priority moves
                priorityMove = priorityMoves[rand.Next() % priorityMoves.Count];
            }

            // return the selection
            return (priorityMove.Destination == null) ? new string[] { priorityMove.Source } : new string[] { priorityMove.Source, priorityMove.Destination };
        }

        #region private
        private static Random rand = new Random();

        // Locations in the middle of the board are higher priority
        //  indices are numericXalpha
        private static int[][] PrioritySource = new int[][]
                {
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[]{0, 0, 1, 0, 1, 0, 1, 0, 0},
                new int[]{0, 1, 2, 1, 2, 1, 2, 1, 0},
                new int[]{0, 0, 1, 2, 3, 2, 1, 0, 0},
                new int[]{0, 1, 2, 3, 2, 3, 2, 1, 0},
                new int[]{0, 0, 1, 2, 3, 2, 1, 0, 0},
                new int[]{0, 1, 2, 1, 2, 1, 2, 1, 0},
                new int[]{0, 0, 1, 0, 1, 0, 1, 0, 0},
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0}
                };

        // next to rule
        //  indices are srcXdst
        private static int[][] PriorityOfNeighbor = new int[][]
                {
	                    // u  b  f  b1 b2 b1 b2t b2b b2r b2l n1 n2 nn1 nn2
		        new int[] {0, 0, 0, 0, 0, 0, 0,  0,  0,  0,  0, 0, 0,  0}, // Inaccessible
		        new int[] {0, 0, 0, 0, 0, 0, 0,  0,  0,  0,  0, 0, 0,  0}, // Blank
		        new int[] {0, 0, 0, 0, 0, 0, 0,  0,  0,  0,  0, 0, 0,  0}, // FaceDown
		        new int[] {0, 0, 0, 0, 0,17,17, 17, 17, 17,  0, 0, 0,  0}, // Blue_One
		        new int[] {0, 0, 0, 0, 0, 0, 0,  0,  0,  0, 15,13, 0,  0}, // Blue_Two
		        new int[] {0, 0, 0, 0, 0, 0, 0,  0,  0,  0,  0, 0,15, 15}, // Brown_One
		        new int[] {0, 0, 0,19,17, 0, 0,  0,  0,  0, 15,13, 0,  0}, // Brown_Two_Top
		        new int[] {0, 0, 0,19,17, 0, 0,  0,  0,  0, 15,13, 0,  0}, // Brown_Two_Bottom
		        new int[] {0, 0, 0,19,17, 0, 0,  0,  0,  0, 15,13, 0,  0}, // Brown_Two_Right
		        new int[] {0, 0, 0,19,17, 0, 0,  0,  0,  0, 15,13, 0,  0}, // Brown_Two_Left
		        new int[] {0, 0, 0, 0,-5, 0,-5, -5, -5, -5,  0, 0, 0,  0}, // Neutral_One
		        new int[] {0, 0, 0, 0,-5, 0,-5, -5, -5, -5,  0, 0, 0,  0}, // Neutral_Two
		        new int[] {0, 0, 0, 0, 0, 0, 0,  0,  0,  0,  0, 0, 0,  0}, // Neutral_Nonmoving_One
		        new int[] {0, 0, 0, 0, 0, 0, 0,  0,  0,  0,  0, 0, 0,  0}  // Neutral_Nonmoving_Two
		        };

        private static bool IsMine(PlayerColor color, Cards card)
        {
            switch (card)
            {
                case Cards.Inaccessible:
                    return false;
                case Cards.Blank:
                    return false;
                case Cards.FaceDown:
                    return true;
                case Cards.Blue_One:
                    return (color == PlayerColor.Blue);
                case Cards.Blue_Two:
                    return (color == PlayerColor.Blue);
                case Cards.Brown_One:
                    return (color == PlayerColor.Brown);
                case Cards.Brown_Two_Top:
                    return (color == PlayerColor.Brown);
                case Cards.Brown_Two_Bottom:
                    return (color == PlayerColor.Brown);
                case Cards.Brown_Two_Left:
                    return (color == PlayerColor.Brown);
                case Cards.Brown_Two_Right:
                    return (color == PlayerColor.Brown);
                case Cards.Neutral_One:
                    return true;
                case Cards.Neutral_Two:
                    return true;
                case Cards.Neutral_Nonmoving_One:
                    return false;
                case Cards.Neutral_Nonmoving_Two:
                    return false;
            }

            return false;
        }

        // priorities
        private static int PSourceRule(int numeric, int alpha)
        {
            // priority for the source location
            return PrioritySource[numeric][alpha];
        }

        private static int PNextTooRule(PlayerColor color, Cards src, Cards dst, int numericDelta, int alphaDelta)
        {
            if (src != Cards.FaceDown) throw new Exception("Must pass in a source card that is FaceDown");

            // priority of the cards adjacent to this card
            if (color == PlayerColor.Blue)
            {
                switch (dst)
                {
                    case Cards.Blue_One:
                        return 3;
                    case Cards.Blue_Two:
                        return 5;
                    case Cards.Brown_One:
                        return 1;
                    case Cards.Brown_Two_Top:
                        return (numericDelta > 0) ? -5 : 1;
                    case Cards.Brown_Two_Bottom:
                        return (numericDelta < 0) ? -5 : 1;
                    case Cards.Brown_Two_Left:
                        return (alphaDelta > 0) ? -5 : 1;
                    case Cards.Brown_Two_Right:
                        return (alphaDelta < 0) ? -5 : 1;
                    case Cards.Neutral_One:
                        return 7;
                    case Cards.Neutral_Two:
                        return 7;
                    default:
                        return 0;
                }
            }
            else
            {
                switch (dst)
                {
                    case Cards.Blue_One:
                        return -5;
                    case Cards.Blue_Two:
                        return -1;
                    case Cards.Brown_One:
                        return 7;
                    case Cards.Brown_Two_Top:
                        return (numericDelta > 0) ? 12 : -3;
                    case Cards.Brown_Two_Bottom:
                        return (numericDelta < 0) ? 12 : -3;
                    case Cards.Brown_Two_Left:
                        return (alphaDelta > 0) ? 12 : -3;
                    case Cards.Brown_Two_Right:
                        return (alphaDelta < 0) ? 12 : -3;
                    case Cards.Neutral_One:
                        return 5;
                    case Cards.Neutral_Two:
                        return 5;
                    case Cards.Neutral_Nonmoving_One:
                        return 5;
                    case Cards.Neutral_Nonmoving_Two:
                        return 5;
                    default:
                        return 0;
                }
            }
        }

        private static int PMoveToRule(Cards src, Cards dst)
        {
            // assume move is OK
            // priority of the card moving to destination
            return PriorityOfNeighbor[(int)src][(int)dst];
        }

        private static bool IsExit(Cards[][] board, int numeric, int alpha)
        {
            if ((numeric == 0 && alpha == board[0].Length / 2)
                || (numeric == board.Length - 1 && alpha == board[0].Length / 2)
                || (numeric == board.Length / 2 && alpha == 0)
                || (numeric == board.Length / 2 && alpha == board[0].Length - 1)
                ) return true;

            return false;
        }

        private static Move MovePriority(Cards[][] board, bool endPhase, int numeric, int alpha, int numericDelta, int alphaDelta)
        {
            // TODO: Add EndPhase logic

            if (!CardSpec.ValidMove(board[numeric][alpha], board[numeric + numericDelta][alpha + alphaDelta], numericDelta, alphaDelta)) return null;

            // mark as possible move with priority
            var move = new Move();
            move.Source = Move.Location(numeric, alpha);
            move.Destination = Move.Location(numeric + numericDelta, alpha + alphaDelta);
            if (endPhase && IsExit(board, numeric + numericDelta, alpha + alphaDelta)) move.Priority = 20;
            else move.Priority = PMoveToRule(board[numeric][alpha], board[numeric + numericDelta][alpha + alphaDelta]);
            Console.WriteLine($"{move.Source} to {move.Destination} as pri {move.Priority}");

            return move;
        }
        #endregion
    }
}
